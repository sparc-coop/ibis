using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsisTranscriber.NETCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transcriber.Core.Users.Commands;
using Kuvio.Kernel.Core;
using Kuvio.Kernel.Database.CosmosDb;
using Transcriber.Plugins.Cosmos;
using Microsoft.AspNetCore.Components.Authorization;
using Kuvio.Kernel.Storage.Azure;
using Microsoft.AspNetCore.Mvc;

namespace IsisTranscriber.NETCore
{
    public class Startup
    {
        public static int Progress { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
                .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options))
                .OnLogin(principal =>
                  {
                      services.BuildServiceProvider().GetRequiredService<LoginCommand>()
                          .Execute(principal, principal.AzureID(), principal.Email(), principal.DisplayName());
                  });
            services.AddRazorPages();
            services.AddServerSideBlazor();
            //services.AddTransient<AuthenticationStateProvider>();
            //services.AddTransient<UserProvider>();

            AddRepositories(services);
            AddCommands(services);
            AddCosmos(services);

            services.AddControllers(options => options.EnableEndpointRouting = false);
        }

        private void AddRepositories(IServiceCollection services)
        {
            services.AddScoped(options => new StorageContext(Configuration["ConnectionStrings:Storage"]));
            services.AddTransient<IMediaRepository, MediaRepository>();
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

            app.UseMvcWithDefaultRoute();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
            });
        }
    }
}
