using Stripe;

namespace Ibis._Plugins.Billing;
public record PaymentIntentRequest(decimal Amount, string Currency, string? Id = null);
public record TicksPackage(decimal Amount, long Ticks);
public record PaymentIntentResponse(string ClientSecret, string Amount, string Id, string Currency, List<TicksPackage> Amounts);

public class StripeBiller
{
    internal StripeBiller(ExchangeRates exchangeRates, IConfiguration configuration, IRepository<User> users, IRepository<UserCharge> userCharges)
    {
        ExchangeRates = exchangeRates;
        Users = users;
        UserCharges = userCharges;
        EndpointKey = configuration["Stripe:EndpointKey"]!;
    }

    internal ExchangeRates ExchangeRates { get; }
    public IRepository<User> Users { get; }
    public IRepository<UserCharge> UserCharges { get; }
    private string EndpointKey { get; }

    internal async Task<PaymentIntentResponse> CreatePaymentIntentAsync(User user, PaymentIntentRequest request)
    {
        if (user.BillingInfo.CustomerId != null)
        {
            var customer = await new CustomerService().CreateAsync(new()
            {
                Email = user.Email,
                Name = user.Avatar.Name,
                Metadata = new() { { "IbisUserId", user.Id } }
            });
            user.SetUpBilling(customer.Id, request.Currency);
        }

        var amounts = await ExchangeRates.ConvertToNiceAmountsAsync(request.Currency, 1, 5, 10, 20);
        var pricePerMinute = 0.05M;

        List<TicksPackage> packages = new();
        var bonus = 1M;
        foreach (var amt in amounts)
        {
            var amtInUsd = await ExchangeRates.ConvertAsync(amt, request.Currency, "USD");
            var amtInTicks = amtInUsd / pricePerMinute * TimeSpan.TicksPerMinute * bonus;
            packages.Add(new(amt, (long)amtInTicks));
            bonus += 0.1M;
        }

        var selectedAmount = request.Amount == 0 ? amounts.First() : request.Amount;
        var selectedPackage = packages.First(x => x.Amount == selectedAmount);

        var options = new PaymentIntentListOptions
        {
            Customer = user.BillingInfo.CustomerId
        };

        var paymentIntentService = new PaymentIntentService();
        StripeList<PaymentIntent> paymentIntents = paymentIntentService.List(options);

        var paymentIntent = request.Id != null
            ? await paymentIntentService.UpdateAsync(request.Id, new()
            {
                Amount = StripePaymentIntentExtensions.StripeAmount(selectedAmount, request.Currency),
                Metadata = new() { { "Ticks", selectedPackage.Ticks.ToString() } }
            })
            : paymentIntents.Any(x => x.Status != "succeeded")
            ? await paymentIntentService.UpdateAsync(paymentIntents.First(x => x.Status != "succeeded").Id, new()
            {
                Amount = StripePaymentIntentExtensions.StripeAmount(selectedAmount, request.Currency),
                Metadata = new() { { "Ticks", selectedPackage.Ticks.ToString() } }
            })
            :
            await paymentIntentService.CreateAsync(new()
            {
                Amount = StripePaymentIntentExtensions.StripeAmount(selectedAmount, request.Currency),
                Metadata = new() { { "Ticks", selectedPackage.Ticks.ToString() } },
                Customer = user.BillingInfo.CustomerId,
                Currency = request.Currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });

        return new(paymentIntent.ClientSecret, paymentIntent.FormattedAmount(), paymentIntent.Id, paymentIntent.Currency, packages);
    }

    internal async Task<bool> ProcessPaymentIntentAsync(HttpRequest request)
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        try
        {
            var signatureHeader = request.Headers["Stripe-Signature"];
            var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, EndpointKey, throwOnApiVersionMismatch: false);

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                await ProcessPaymentAsync(stripeEvent);
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                await FailPaymentAsync(stripeEvent);
            }

            return true;
        }
        catch (StripeException)
        {
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task FailPaymentAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        var customer = new CustomerService().Get(paymentIntent!.CustomerId);
        var user = await Users.FindAsync(customer.Metadata["IbisUserId"]);
        if (user != null)
        {
            UserCharge userCharge = new(user.Id, paymentIntent!);
            await UserCharges.AddAsync(userCharge);
        }
    }

    private async Task ProcessPaymentAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        var customer = new CustomerService().Get(paymentIntent!.CustomerId);
        var user = await Users.FindAsync(customer.Metadata["IbisUserId"]);
        if (user != null)
        {
            UserCharge userCharge = new(user.Id, paymentIntent!);
            await Users.ExecuteAsync(user, x => x.Refill(userCharge.Ticks));
            await UserCharges.AddAsync(userCharge);
        }
    }
}

