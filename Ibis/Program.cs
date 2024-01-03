using Ibis;
using Stripe;
using Sparc.Ibis;
using Ibis.UI;

var app = BlossomApplication.Run<App, User, IbisHub>(args,
    (services, configuration) =>
    {
        services.AddCosmos<IbisContext>(configuration)
                .AddAzureStorage(configuration)
                .AddTwilio(configuration)
                .AddIbis(configuration)
                .AddScoped<ExchangeRates>()
                .AddScoped<ITranslator, AzureTranslator>()
                .AddScoped<ISpeaker, AzureSpeaker>()
                .AddScoped<IListener, AzureListener>()
                .AddScoped<GetAllContent>();

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    },
    (app) =>
    {
        app.UseAllCultures();
    });