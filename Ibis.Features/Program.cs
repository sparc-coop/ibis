using Ibis;
using Lamar.Microsoft.DependencyInjection;
using Stripe;
using Sparc.Ibis;
using Blazored.Modal;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

builder.AddBlossom(builder.Configuration["WebClientUrl"]);

builder.Services
        .AddCosmos<IbisContext>(builder.Configuration.GetConnectionString("Database")!, "ibis-prod", ServiceLifetime.Transient)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddTwilio(builder.Configuration)
        .AddBlossomRealtime<IbisHub>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>()
        .AddScoped<IListener, AzureListener>()
        .AddSingleton<ExchangeRates>()
        .AddScoped<GetAllContent>()
        .AddScoped<AzureLanguageDetector>();


builder.Services.AddBlazoredModal();

builder.AddBlossomAuthentication<User>();

builder.Services.AddIbis(builder.Configuration["IbisApi"]!);
builder.Services.AddServerSideBlazor();
builder.Services.AddOutputCache();

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseBlossom();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToFile("index.html");

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.MapHub<IbisHub>("/hub");
app.UseBlossomAuthentication<User>();
app.UseAllCultures();

// Warm up the entity framework model
//_ = app.Services.GetRequiredService<IbisContext>().Model;

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
