using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IbisTranscriber.Data;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using IbisTranscriber.Extensions;
using Kuvio.Kernel.Core;
using Transcriber.Core.Users.Commands;
using Kuvio.Kernel.Database.CosmosDb;
using Transcriber.Plugins.Cosmos;

namespace IbisTranscriber
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
                .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));
                //.OnLogin(principal =>
                // {
                //     services.BuildServiceProvider().GetRequiredService<LoginCommand>()
                //         .Execute(principal, principal.AzureID(), principal.Email(), principal.DisplayName());
                // });

            services.AddControllersWithViews();
            //services.AddControllersWithViews(options =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();
            //    options.Filters.Add(new AuthorizeFilter(policy));
            //});

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            services.AddTransient<UserProvider>();

            AddCommands(services);
            AddCosmos(services);
        }

        private void AddCosmos(IServiceCollection services)
        {
            var db = new CosmosDbOptions();
            Configuration.Bind("CosmosDb", db);
            services.AddCosmosContext<CosmosContext>(db);
            services.AddTransient(typeof(IRepository<>), typeof(CosmosDbRepository<>));
        }

        private void AddCommands(IServiceCollection services)
        {
            services.AddTransient<LoginCommand>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
