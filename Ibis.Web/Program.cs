using Ibis.Features;
using Ibis.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sparc.Authentication.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<IConfiguration>(_ => builder.Configuration);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.AddB2CApi<IbisApi>(
        "https://ibis.onmicrosoft.com/ccba7246-6276-4566-a964-12d7a2b48198/IbisAPI.Access",
        builder.Configuration["ApiUrl"]);

await builder.Build().RunAsync();
