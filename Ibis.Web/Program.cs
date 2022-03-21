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
        "https://ibisapp.onmicrosoft.com/b270db9b-f943-45fd-b912-d17920a83fd5/Ibis.Features",
        builder.Configuration["ApiUrl"]);

await builder.Build().RunAsync();
