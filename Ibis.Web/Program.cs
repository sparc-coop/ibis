using Ibis.Api;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.Modal;
using Sparc.Ibis;
using Sparc.Blossom.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredModal();
builder.Services.AddIbis();

var apiUrl = builder.Configuration["ApiUrl"];
builder.AddBlossom<IbisApi>(builder.Configuration["ApiUrl"]);

await builder.Build().RunAsync();
