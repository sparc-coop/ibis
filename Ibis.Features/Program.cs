using Ibis._Plugins.Billing;
using Ibis._Plugins.Blossom;
using Ibis._Plugins.Database;
using Ibis._Plugins.Realtime;
using Ibis._Plugins.Speech;
using Ibis._Plugins.Translation;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

var app = builder.Blossom<User>(s => 
    s.AddCosmos<IbisContext>(builder.Configuration.GetConnectionString("Database")!, "ibis-prod", ServiceLifetime.Transient)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddTwilio(builder.Configuration)
        .AddBlossomRealtime<IbisHub>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>()
        .AddScoped<IListener, AzureListener>()
        .AddSingleton<ExchangeRates>());

app.Host<Ibis.Chat.App>("ibis.chat", 5001, "Chat");
app.Host<Ibis.Ink.App>("ibis.ink", 5002, "Ink");
app.Host<Ibis.Support.App>("ibis.support", 5003, "Support");

app.MapHub<IbisHub>("/hub");
app.UseAllCultures();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
