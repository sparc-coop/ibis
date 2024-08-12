using Ibis;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;


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
        .AddScoped<PostContent>()
        .AddScoped<TypeMessage>()
        .AddScoped<UploadFile>();

builder.Services.AddOutputCache();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

//app.UseAllCultures();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.MapPost("/publicapi/PostContent", async (PostContentRequest request, PostContent postContent) =>
{
    return await postContent.ExecuteAsync(request);
});

app.MapPost("/publicapi/TypeMessage", async (TypeMessageRequest request, TypeMessage typeMessage) =>
{
    return await typeMessage.ExecuteAsUserAsync(request, User.System);
});

app.MapPost("/publicapi/UploadImage", async ([FromForm]UploadFileRequest request, UploadFile uploadFile, IFormFile file) =>
{
    return await uploadFile.ExecuteAsync(request);
});

app.MapGet("/publicapi/Languages", async (Translator translator) =>
{
    return await translator.GetLanguagesAsync();
});

await app.RunAsync();
