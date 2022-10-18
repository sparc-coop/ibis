﻿using Ibis.Features.Sparc.Realtime;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Sparc.Authentication.AzureADB2C;
using Sparc.Notifications.Twilio;
using Sparc.Plugins.Database.Cosmos;
using Sparc.Storage.Azure;

namespace Ibis.Features;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Sparcify<Startup>(Configuration["WebClientUrl"])
            .AddAzureADB2CAuthentication(Configuration)
            .AddAzureStorage(Configuration.GetConnectionString("Storage"))
            .AddTwilio(Configuration)
            .AddSparcRealtime<IbisHub>(Configuration.GetConnectionString("SignalR"));

        // Bug fix for Sparc Realtime (events executing in parallel with a scoped context)
        services.AddDbContext<IbisContext>(options => options.UseCosmos(Configuration.GetConnectionString("Database"), "ibis", options =>
        {
            options.ConnectionMode(ConnectionMode.Direct);
        }), ServiceLifetime.Transient);
        services.AddTransient(typeof(DbContext), typeof(IbisContext));
        services.AddTransient(sp => new CosmosDbDatabaseProvider(sp.GetRequiredService<DbContext>(), "ibis"));

        services.AddTransient(typeof(IRepository<>), typeof(CosmosDbRepository<>))
            .AddScoped<ITranslator, AzureTranslator>()
            .AddScoped<ISpeaker, AzureSpeaker>();

        services.AddRazorPages();

        services.AddSingleton<HttpClient>(sp =>
        {
            // Get the address that the app is currently running at
            var server = sp.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            string baseAddress = addressFeature.Addresses.First();
            return new HttpClient { BaseAddress = new Uri(baseAddress) };
        });

        services.AddScoped<HttpClient>(s =>
        {
            var navigationManager = s.GetRequiredService<NavigationManager>();
            return new HttpClient
            {
                BaseAddress = new Uri(navigationManager.BaseUri)
            };
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Sparcify<Startup>(env);
        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapHub<IbisHub>("/hub");
            endpoints.MapFallbackToPage("/_Host");
        });
        app.UseDeveloperExceptionPage();
    }
}
