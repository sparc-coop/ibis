using Ibis;
using Stripe;
using Sparc.Ibis;
using Blazored.Modal;

var app = BlossomApplication.Run<App, User, IbisHub>(args, 
    (services, configuration) =>
    {
        services.AddCosmos<IbisContext>(configuration)
                .AddAzureStorage(configuration)
                .AddTwilio(configuration)
                .AddScoped<ITranslator, AzureTranslator>()
                .AddScoped<ISpeaker, AzureSpeaker>()
                .AddScoped<IListener, AzureListener>()
                .AddSingleton<ExchangeRates>()
                .AddScoped<GetAllContent>()
                .AddBlazoredModal()
                .AddIbis(configuration["IbisApi"]!);

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }, 
    (app) =>
    {
        app.UseAllCultures();
    });
