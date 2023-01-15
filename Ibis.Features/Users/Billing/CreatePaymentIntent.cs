using Stripe;

namespace Ibis.Features.Users;

public record PaymentIntentRequest(decimal Amount, string Currency, string? Id = null);
public record PaymentIntentResponse(string ClientSecret, string Amount, string Id, string Currency, List<decimal> Amounts);
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

        if (user.BillingInfo == null)
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
        var amount = request.Amount == 0 ? amounts.First() : request.Amount;

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = request.Id != null
            ? await paymentIntentService.UpdateAsync(request.Id, new()
            {
                Amount = StripePaymentIntentExtensions.StripeAmount(amount, request.Currency)
            })
            :
            await paymentIntentService.CreateAsync(new()
            {
                Amount = StripePaymentIntentExtensions.StripeAmount(amount, request.Currency),
                Customer = user.BillingInfo!.CustomerId,
                Currency = request.Currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });

        return new(paymentIntent.ClientSecret, paymentIntent.FormattedAmount(), paymentIntent.Id, paymentIntent.Currency, amounts);
    }


}
