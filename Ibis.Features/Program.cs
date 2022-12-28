using Ibis.Features;
using Lamar.Microsoft.DependencyInjection;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

builder.AddBlossom(builder.Configuration["WebClientUrl"]);

builder.Services
        .AddCosmos<IbisContext>(builder.Configuration.GetConnectionString("Database")!, "ibis", ServiceLifetime.Transient)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddTwilio(builder.Configuration)
        .AddBlossomRealtime<IbisHub>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>()
        .AddScoped<IListener, AzureListener>()
        .AddSingleton<ExchangeRates>();

builder.Services.RegisterAggregates();

var auth = builder.Services.AddAzureADB2CAuthentication<User>(builder.Configuration);
builder.AddPasswordlessAuthentication<User>(auth);

var app = builder.BuildBlossom();

app.MapHub<IbisHub>("/hub");
app.UsePasswordlessAuthentication<User>();
app.UseAllCultures();
app.MapAggregates();

// Warm up the entity framework model
//_ = app.Services.GetRequiredService<IbisContext>().Model;

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
