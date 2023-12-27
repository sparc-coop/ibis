using Stripe;

namespace Ibis.Users;

public record PaymentIntentRequest(decimal Amount, string Currency, string? Id = null);
public record TicksPackage(decimal Amount, long Ticks);
public record PaymentIntentResponse(string ClientSecret, string Amount, string Id, string Currency, List<TicksPackage> Amounts);

public class CreatePaymentIntent : Feature<PaymentIntentRequest, PaymentIntentResponse>
{
    public IConfiguration Configuration { get; }
    public IRepository<User> Users { get; }
    public ExchangeRates ExchangeRates { get; }

    public CreatePaymentIntent(IConfiguration configuration, IRepository<User> users, ExchangeRates exchangeRates)
    {
        Configuration = configuration;
        Users = users;
        ExchangeRates = exchangeRates;
    }

    public override async Task<PaymentIntentResponse> ExecuteAsync(PaymentIntentRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
            throw new NotAuthorizedException("Can't find user information!");

        if (user.BillingInfo.CustomerId == null)
        {
            var customer = await new CustomerService().CreateAsync(new()
            {
                Email = user.Email,
                Name = user.Avatar.Name,
                Metadata = new() { { "IbisUserId", user.Id } }
            });
            user.SetUpBilling(customer.Id, request.Currency);
            await Users.UpdateAsync(user);
        }

        var amounts = await ExchangeRates.ConvertToNiceAmountsAsync(request.Currency, 1, 5, 10, 20);
        var pricePerMinute = 0.05M;
        
        List<TicksPackage> packages = [];
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
}
