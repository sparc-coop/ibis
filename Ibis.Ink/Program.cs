using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using Blazored.Modal;
using Sparc.Ibis;
using Ibis.Ink;
using Sparc.Blossom;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredModal();
builder.Services.AddIbis();

builder.AddBlossom<IbisClient>(builder.Configuration["Blossom:Authority"]);

await builder.Build().RunAsync();