using Stripe;

namespace Ibis.Features.Users
{
    public class ProcessPaymentIntent : PublicFeature<bool>
    {
        readonly string EndpointKey;
        
        public ProcessPaymentIntent(IConfiguration configuration, IRepository<User> users, IRepository<UserCharge> userCharges)
        {
            EndpointKey = "whsec_5e3430a88cd1384aa9573526e353ffc1948b79f19c37c5a4227f70659eb41770";//configuration["Stripe:EndpointKey"]!;
            Users = users;
            UserCharges = userCharges;
        }

        public IRepository<User> Users { get; }
        public IRepository<UserCharge> UserCharges { get; }

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
            catch (StripeException e)
            {
                return false;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task FailPaymentAsync(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            var user = Users.Query.FirstOrDefault(x => x.CustomerId == paymentIntent!.CustomerId);
            if (user != null)
            {
                UserCharge userCharge = new(user.Id, paymentIntent!);
                await UserCharges.AddAsync(userCharge);
            }
        }

        private async Task ProcessPaymentAsync(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            var user = Users.Query.FirstOrDefault(x => x.CustomerId == paymentIntent!.CustomerId);
            if (user != null)
            {
                UserCharge userCharge = new(user.Id, paymentIntent!);
                await Users.ExecuteAsync(user, x => x.AddCharge(userCharge));
                await UserCharges.AddAsync(userCharge);
            }
        }
    }
}
