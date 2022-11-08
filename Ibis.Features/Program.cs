using Ibis.Features;
using Lamar.Microsoft.DependencyInjection;
using Sparc.Authentication.AzureADB2C;
using Sparc.Database.Cosmos;
using Sparc.Storage.Azure;
using Sparc.Notifications.Twilio;
using Stripe;
using Sparc.Authentication;
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
        .AddSingleton<ExchangeRates>();

var auth = builder.Services.AddAzureADB2CAuthentication<User>(builder.Configuration);
builder.AddPasswordlessAuthentication<User>(auth);

builder.Services.AddServerSideBlazor();

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseSparcKernel();
app.MapControllers();
app.MapHub<IbisHub>("/hub");
app.MapBlazorHub();
app.MapFallbackToFile("index.html");
app.UseDeveloperExceptionPage();
app.UsePasswordlessAuthentication<User>();

// Warm up the entity framework model
_ = app.Services.GetRequiredService<IbisContext>().Model;

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
