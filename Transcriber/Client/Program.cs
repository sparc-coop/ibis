using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Kuvio.Kernel.AspNet.Blazor;
using Kuvio.Kernel.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Transcriber.Core.Users.Commands;
using Kuvio.Kernel.Database.CosmosDb;
using Transcriber.Plugins.Cosmos;

namespace Transcriber.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.AddHttpClient("Transcriber.Server.ServerAPI", client =>
            //    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            //    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            //builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
            //    .CreateClient("Transcriber.Server.ServerAPI"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                //options.ProviderOptions.DefaultAccessTokenScopes.Add("https://ibistranscriber.onmicrosoft.com/80f12d5d-6f3a-4e1b-ab25-47ddc6dbd72a/API.Access");
                options.UserOptions.NameClaim = "name";
            });

            builder.Services.AddBlazorModal();

            builder.Services.AddBlazorToast(options =>
            {
                options.Timeout = 5; // default: 5
                options.Position = Kuvio.Kernel.AspNet.Blazor.Toast.Configuration.ToastPosition.TopRight; // default: ToastPosition.TopRight
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });


            var db = new CosmosDbOptions();
        
            builder.Configuration.Bind("CosmosDb", db);
            


            builder.Services.AddCosmosContext<CosmosContext>(db);
            builder.Services.AddTransient(typeof(IRepository<>), typeof(CosmosDbRepository<>));

            builder.Services.AddTransient<UserProvider>();
            builder.Services.AddTransient<LoginCommand>();

            await builder.Build().RunAsync();
        }
    }
}
