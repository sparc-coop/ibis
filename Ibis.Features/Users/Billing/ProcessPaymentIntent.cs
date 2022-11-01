using Stripe;

namespace Ibis.Features.Users
{
    public class ProcessPaymentIntent : PublicFeature<bool>
    {
        readonly string EndpointKey;
        
        public ProcessPaymentIntent(IConfiguration configuration, IRepository<User> users, IRepository<UserCharge> userCharges, ExchangeRates exchangeRates)
        {
            EndpointKey = configuration["Stripe:EndpointKey"]!;
            Users = users;
            UserCharges = userCharges;
            ExchangeRates = exchangeRates;
        }

        public IRepository<User> Users { get; }
        public IRepository<UserCharge> UserCharges { get; }
        public ExchangeRates ExchangeRates { get; }

        public override async Task<bool> ExecuteAsync()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var signatureHeader = Request.Headers["Stripe-Signature"];
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, EndpointKey);

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
                await Users.ExecuteAsync(user, x => x.AddCharge(userCharge));
                await UserCharges.AddAsync(userCharge);
            }
        }
    }
}
