using Ibis.Features;
using Lamar.Microsoft.DependencyInjection;
using Sparc.Authentication.AzureADB2C;
using Sparc.Database.Cosmos;
using Sparc.Storage.Azure;
using Sparc.Notifications.Twilio;
using Stripe;
using Sparc.Authentication;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

builder.AddSparcKernel(builder.Configuration["WebClientUrl"]);

builder.Services
        .AddCosmos<IbisContext>(builder.Configuration.GetConnectionString("Database")!, "ibis", ServiceLifetime.Transient)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddTwilio(builder.Configuration)
        .AddSparcRealtime<IbisHub>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>()
        .AddScoped<IListener, AzureListener>()
        .AddSingleton<ExchangeRates>()
        .AddTransient<IClaimsTransformation, AzureAdB2CClaimsTransformation>();

var auth = builder.Services.AddAzureADB2CAuthentication(builder.Configuration);
builder.AddPasswordlessAuthentication<User>(auth);

var app = builder.Build();

app.UseSparcKernel();
app.MapControllers();
app.MapHub<IbisHub>("/hub");
app.UseDeveloperExceptionPage();
app.UsePasswordlessAuthentication<User>();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();