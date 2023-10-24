using Ibis;
using Lamar.Microsoft.DependencyInjection;
using Stripe;
using Sparc.Ibis;

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
        .AddScoped<GetAllContent>();

var auth = builder.Services.AddAzureADB2CAuthentication<User>(builder.Configuration);
auth.AddCookie();
builder.AddPasswordlessAuthentication<User>(auth);

builder.Services.AddIbis(builder.Configuration["IbisApi"]!);
builder.Services.AddServerSideBlazor();
builder.Services.AddOutputCache();

var app = builder.Build();

app.MapWhen(ctx => ctx.Request.Host.Port == 5001 ||
    ctx.Request.Host.Equals("ibis.chat"), first =>
    {
        first.Use((ctx, nxt) =>
        {
            ctx.Request.Path = "/Chat" + ctx.Request.Path;
            return nxt();
        });

        first.UseBlazorFrameworkFiles("/Chat");
        first.UseStaticFiles();
        first.UseStaticFiles("/Chat");
        first.UseRouting();

        first.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("/Chat/{*path:nonfile}",
                "Chat/index.html");
        });
    });

app.MapWhen(ctx => ctx.Request.Host.Port == 5002 ||
    ctx.Request.Host.Equals("ibis.ink"), second =>
    {
        second.Use((ctx, nxt) =>
        {
            ctx.Request.Path = "/Ink" + ctx.Request.Path;
            return nxt();
        });

        second.UseBlazorFrameworkFiles("/Ink");
        second.UseStaticFiles();
        second.UseStaticFiles("/Ink");
        second.UseRouting();

        second.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("/Ink/{*path:nonfile}",
                "Ink/index.html");
        });
    });

app.UseBlossom();
app.MapControllers();
app.MapBlazorHub();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.MapHub<IbisHub>("/hub");
app.UsePasswordlessAuthentication<User>();
app.UseAllCultures();

// Warm up the entity framework model
//_ = app.Services.GetRequiredService<IbisContext>().Model;

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await app.RunAsync();
