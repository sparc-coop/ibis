using Ibis.Features;
using Ibis.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.Modal;
using Sparc.Ibis;
using Sparc.Blossom.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredModal();
builder.Services.AddIbis();

var b2c = builder.Configuration["AzureAdB2C:Scope"];

builder.AddBlossom<IbisApi>(builder.Configuration["ApiUrl"]);

await builder.Build().RunAsync();
