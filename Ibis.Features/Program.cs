using Ibis;
using Ibis._Plugins.Blossom;
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
        .AddSingleton<ExchangeRates>()
        .AddScoped<GetAllContent>());

app.Host("ibis.chat", 5001, "Chat", RenderMode.WebAssembly);
app.Host("ibis.ink", 5002, "Ink", RenderMode.WebAssembly);
app.Host("ibis.support", 5003, "Support", RenderMode.WebAssembly);

app.MapHub<IbisHub>("/hub");
app.UseAllCultures();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
