using Sparc.Core;
using Sparc.Features;
using Stripe;

namespace Ibis.Features.Users
{
    public class CreateStripeCustomer : Feature<User, bool>
    {
        public IConfiguration? Configuration;

        public override async Task<bool> ExecuteAsync(User request)
        {
            try
            {
                StripeConfiguration.ApiKey = Configuration["Strip:ApiKey"];

                var options = new CustomerCreateOptions { };
                var service = new CustomerService();
                var customer = service.Create(options);
                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }


        private void FutureBilling(string customerId)
        {
            StripeConfiguration.ApiKey = Configuration["Strip:ApiKey"];
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
