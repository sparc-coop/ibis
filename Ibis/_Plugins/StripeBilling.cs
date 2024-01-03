using Stripe;
using System.Globalization;

namespace Ibis._Plugins;

public class StripeBilling(ExchangeRates exchangeRates)
{
    public ExchangeRates ExchangeRates { get; } = exchangeRates;

    public async Task CreateCustomerAsync(User user, string currency)
    {
        var customer = await new CustomerService().CreateAsync(new()
        {
            Email = user.Email,
            Name = user.Avatar.Name,
            Metadata = new() { { "IbisUserId", user.Id } }
        });

        user.SetUpBilling(customer.Id, currency);
    }
    public record PaymentIntentResponse(string ClientSecret, string Amount, string Id, string Currency, List<TicksPackage> Amounts);
    public record TicksPackage(decimal Amount, long Ticks);

    public async Task<PaymentIntentResponse> StartPaymentAsync(string customerId, decimal amount, string currency, string? paymentIntentId = null)
    {
        var amounts = await ExchangeRates.ConvertToNiceAmountsAsync(currency, 1, 5, 10, 20);
        var pricePerMinute = 0.05M;

        List<TicksPackage> packages = [];
        var bonus = 1M;
        foreach (var amt in amounts)
        {
            var amtInUsd = await ExchangeRates.ConvertAsync(amt, currency, "USD");
            var amtInTicks = amtInUsd / pricePerMinute * TimeSpan.TicksPerMinute * bonus;
            packages.Add(new(amt, (long)amtInTicks));
            bonus += 0.1M;
        }

        var selectedAmount = amount == 0 ? amounts.First() : amount;
        var selectedPackage = packages.First(x => x.Amount == selectedAmount);

        var options = new PaymentIntentListOptions
        {
            Customer = customerId
        };

        var paymentIntentService = new PaymentIntentService();
        StripeList<PaymentIntent> paymentIntents = paymentIntentService.List(options);

        var paymentIntent = paymentIntentId != null
            ? await paymentIntentService.UpdateAsync(paymentIntentId, new()
            {
                Amount = StripeAmount(selectedAmount, currency),
                Metadata = new() { { "Ticks", selectedPackage.Ticks.ToString() } }
            })
            : paymentIntents.Any(x => x.Status != "succeeded")
            ? await paymentIntentService.UpdateAsync(paymentIntents.First(x => x.Status != "succeeded").Id, new()
            {
                Amount = StripeAmount(selectedAmount, currency),
                Metadata = new() { { "Ticks", selectedPackage.Ticks.ToString() } }
            })
            :
            await paymentIntentService.CreateAsync(new()
            {
                Amount = StripeAmount(selectedAmount, currency),
                Metadata = new() { { "Ticks", selectedPackage.Ticks.ToString() } },
                Customer = customerId,
                Currency = currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });

        return new(paymentIntent.ClientSecret, FormattedAmount(paymentIntent), paymentIntent.Id, paymentIntent.Currency, packages);
    }

    internal async Task<UserCharge?> CompletePaymentAsync(User user, HttpRequest request, string endpointKey)
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();

        try
        {
            var signatureHeader = request.Headers["Stripe-Signature"];
            var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointKey, throwOnApiVersionMismatch: false);
            var userCharge = stripeEvent.Type == Events.PaymentIntentSucceeded
                ? UserCharge.Process(stripeEvent, user)
                : UserCharge.Fail(stripeEvent);

            return userCharge;
        }
        catch (StripeException)
        {
            return null;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static long StripeAmount(decimal amount, string currency)
    {
        return (long)(amount * StripeCurrencyMultiplier(currency));
    }

    public static decimal LocalAmount(PaymentIntent paymentIntent)
    {
        return paymentIntent.Amount / StripeCurrencyMultiplier(paymentIntent.Currency);
    }


    public static string FormattedAmount(PaymentIntent paymentIntent)
    {
        TryGetCurrencySymbol(paymentIntent.Currency, out string? symbol);
        return $"{symbol ?? paymentIntent.Currency.ToUpper()}{paymentIntent.Amount / StripeCurrencyMultiplier(paymentIntent.Currency):0.##}";
    }

    static readonly List<string> ZeroDecimalCurrencies = ["bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"];

    static decimal StripeCurrencyMultiplier(string currency) => ZeroDecimalCurrencies.Contains(currency.ToLower()) ? 1 : 100;

    static bool TryGetCurrencySymbol(string ISOCurrencySymbol, out string? symbol)
    {
        ISOCurrencySymbol = ISOCurrencySymbol.ToUpper();

        symbol = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Where(c => !c.IsNeutralCulture)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch
                {
                    return null;
                }
            })
            .Where(ri => ri != null && ri.ISOCurrencySymbol == ISOCurrencySymbol)
            .Select(ri => ri!.CurrencySymbol)
            .FirstOrDefault();
        return symbol != null;
    }
}
