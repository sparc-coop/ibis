using Ibis.Features;
using Lamar.Microsoft.DependencyInjection;
using Sparc.Authentication.AzureADB2C;
using Sparc.Database.Cosmos;
using Sparc.Storage.Azure;
using Sparc.Notifications.Twilio;
using Stripe;
using Sparc.Authentication;
using Blazored.Modal;
using Sparc.Blossom.Web;
using Ibis.Api;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Components;

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

// **** Prerendering dependencies for Ibis.Web
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<Device, WebDevice>();
builder.Services.AddScoped(_ => builder.Configuration);
builder.Services.TryAddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddBlossomHttpClient<IbisApi>(builder.Configuration["ApiUrl"]);

// **** End prerendering dependencies for Ibis.Web

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseSparcKernel();
app.MapControllers();
app.MapHub<IbisHub>("/hub");
app.MapFallbackToPage("/_Host");
app.UseDeveloperExceptionPage();
app.UsePasswordlessAuthentication<User>();

// Warm up the entity framework model
_ = app.Services.GetRequiredService<IbisContext>().Model;

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
