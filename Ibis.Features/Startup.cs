using Ibis.Features.Sparc.Realtime;
using Sparc.Authentication.AzureADB2C;
using Sparc.Notifications.Twilio;
using Sparc.Plugins.Database.Cosmos;
using Sparc.Storage.Azure;

namespace Ibis.Features
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Sparcify<Startup>(Configuration["WebClientUrl"])
                .AddCosmos<IbisContext>(Configuration.GetConnectionString("Database"), "ibis")
                .AddAzureADB2CAuthentication(Configuration)
                .AddAzureStorage(Configuration.GetConnectionString("Storage"))
                .AddTwilio(Configuration)
                .AddSparcRealtime<Startup>();

            services.AddScoped(typeof(IRepository<>), typeof(CosmosDbRepository<>))
                .AddScoped<ITranslator, AzureTranslator>()
                .AddScoped<ISpeaker, AzureSpeaker>();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Sparcify<Startup>(env);
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHub<IbisHub>("/hub");
                });
            app.UseDeveloperExceptionPage();
        }
    }
}
