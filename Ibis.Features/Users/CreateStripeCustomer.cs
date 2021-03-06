using Newtonsoft.Json;
using Stripe;

namespace Ibis.Features.Users;

public record CreateStripeCustomerRequest(string userId);
public class CreateStripeCustomer : Feature<CreateStripeCustomerRequest, string>
{
    public IConfiguration? Configuration { get; }
    public IRepository<User> Users { get; }

    public CreateStripeCustomer(IConfiguration configuration, IRepository<User> users)
    {
        Configuration = configuration;
        Users = users;
    }

    public override async Task<string> ExecuteAsync(CreateStripeCustomerRequest request)
    {
        StripeConfiguration.ApiKey = Configuration["Stripe:ApiKey"];

        User user = await Users.FindAsync(request.userId);
        string intent = "";
        var service = new CustomerService();

        if (string.IsNullOrEmpty(user.CustomerId))
        {
            var options = new CustomerCreateOptions
            {
                Name = user.FullName,
                Email = user.Email
            };

            var customer = service.Create(options);
            intent = await SetupIntent(customer.Id);

            user.CustomerId = customer.Id;
            await Users.UpdateAsync(user);
        }
        else
        {
            intent = "current-customer";
        }

        return JsonConvert.SerializeObject(intent);
    }


    private async Task<string> SetupIntent(string customerId)
    {
        StripeConfiguration.ApiKey = Configuration["Stripe:ApiKey"];
        var options = new SetupIntentCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
        };
        var service = new SetupIntentService();
        var intent = await service.CreateAsync(options);

        return intent.ClientSecret;
    }
}
