using Ibis;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery;
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
//builder.Services.AddAntiforgery();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

//app.UseAllCultures();
//app.UseAntiforgery();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// If wanting to use the anti-forgery token
//app.MapGet("/antiforgery/token", (HttpContext httpContext, IAntiforgery antiforgery) =>
//{
//    var tokens = antiforgery.GetAndStoreTokens(httpContext);
//    httpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false });
//    return Results.Ok(new { token = tokens.RequestToken });
//});

app.MapPost("/publicapi/PostContent", async (PostContentRequest request, PostContent postContent) =>
{
    return await postContent.ExecuteAsync(request);
});

app.MapPost("/publicapi/TypeMessage", async (TypeMessageRequest request, TypeMessage typeMessage) =>
{
    return await typeMessage.ExecuteAsUserAsync(request, User.System);
});

app.MapPost("/publicapi/UploadImage", async ([FromForm]UploadFileRequest request, UploadFile uploadFile) =>
{
    return await uploadFile.ExecuteAsync(request);
}).DisableAntiforgery(); 

app.MapGet("/publicapi/Languages", async (Translator translator) =>
{
    return await translator.GetLanguagesAsync();
});

await app.RunAsync();
