using Stripe;

namespace Ibis.Features.Users;

public record PaymentIntentRequest(decimal Amount, string Currency);
public record PaymentIntentResponse(string ClientSecret);
public class CreatePaymentIntent : Feature<PaymentIntentRequest, PaymentIntentResponse>
{
    public IConfiguration Configuration { get; }
    public IRepository<User> Users { get; }

    public CreatePaymentIntent(IConfiguration configuration, IRepository<User> users)
    {
        Configuration = configuration;
        Users = users;
    }

    public override async Task<PaymentIntentResponse> ExecuteAsync(PaymentIntentRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
            throw new NotAuthorizedException("Can't find user information!");

        if (user.CustomerId == null)
        {
            var customer = await new CustomerService().CreateAsync(new()
            {
                Email = user.Email,
                Name = user.Avatar.Name,
                Metadata = new() { { "IbisUserId", user.Id } }
            });
            user.CustomerId = customer.Id;
            await Users.UpdateAsync(user);
        }
        
        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)request.Amount * 100,
            Customer = user.CustomerId,
            Currency = request.Currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        });

        return new(paymentIntent.ClientSecret);
    }
}
