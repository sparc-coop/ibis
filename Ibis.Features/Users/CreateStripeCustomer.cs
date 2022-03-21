using Sparc.Core;
using Sparc.Features;
using Stripe;

namespace Ibis.Features.Users
{
    public class CreateStripeCustomer : Feature<bool>//Feature<User, bool>
    {
        public IConfiguration Configuration;

        public CreateStripeCustomer(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public override async Task<bool> ExecuteAsync()//User request)
        {
            try
            {
                StripeConfiguration.ApiKey = Configuration["Stripe:ApiKey"];

                //var options = new CustomerCreateOptions { };
                var options = new CustomerCreateOptions { 
                    Name = "New Customer",
                    Email = "email@email.com"
                };
                var service = new CustomerService();
                var customer = service.Create(options);

                FutureBilling(customer.Id);
                
                //request.CustomerId = customer.Id;   
                //save user request

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }


        private void FutureBilling(string customerId)
        {
            StripeConfiguration.ApiKey = Configuration["Stripe:ApiKey"];
            var options = new SetupIntentCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
            };
            var service = new SetupIntentService();
            service.Create(options);
        }
    }
}
