using Ibis;
using Lamar.Microsoft.DependencyInjection;
using Stripe;


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

builder.Services
        .AddCosmos<IbisContext>(builder.Configuration.GetConnectionString("Database")!, "ibis-prod", ServiceLifetime.Transient)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddScoped<Translator>()
        .AddScoped<ITranslator, DeepLTranslator>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>()
        .AddScoped<IListener, AzureListener>()
        .AddSingleton<ExchangeRates>()
        .AddScoped<GetAllContent>()
        .AddScoped<PostContent>();

builder.Services.AddOutputCache();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

//app.UseAllCultures();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.MapPost("/publicapi/GetAllContent", async (GetAllContentRequest request, GetAllContent getAllContent) =>
{
    return await getAllContent.ExecuteAsync(request);
});

app.MapPost("/publicapi/PostContent", async (PostContentRequest request, PostContent postContent) =>
{
    return await postContent.ExecuteAsync(request);
});

await app.RunAsync();
