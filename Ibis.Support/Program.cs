using Ibis.Support;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sparc.Blossom;
using Sparc.Ibis;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddIbis();
builder.AddBlossom<IbisClient>(builder.Configuration["Blossom:Authority"]);

await builder.Build().RunAsync();
