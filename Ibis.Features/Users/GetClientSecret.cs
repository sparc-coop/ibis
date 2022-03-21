using Stripe;
using System;
using Microsoft.AspNetCore.Mvc;
using Sparc.Features;

namespace Ibis.Features
{
    public class GetClientSecret : PublicFeature<string>
    {
        public IConfiguration Configuration;
        public GetClientSecret(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public override async Task<string> ExecuteAsync()
        {
            try
            {
                StripeConfiguration.ApiKey = Configuration["Stripe:ApiKey"];
                var options = new PaymentIntentCreateOptions
                {
                    //Amount = 1099,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> {
                        "card",
                    },
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);
                return intent.ClientSecret;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
