using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.Modal;
using Sparc.Ibis;
using Ibis.Chat;
using Microsoft.AspNetCore.Components.Web;
using Sparc.Blossom;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredModal();
builder.Services.AddIbis();

builder.AddBlossom<IbisClient>(builder.Configuration["ApiUrl"]);

await builder.Build().RunAsync();
