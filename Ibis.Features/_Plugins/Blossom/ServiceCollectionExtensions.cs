﻿namespace Ibis._Plugins.Blossom;

public static class ServiceCollectionExtensions
{
    public static WebApplication Host(this WebApplication app, string domainName, int developmentPort, string staticWebAssetBasePath)
    {
        app.MapWhen(x => x.Request.Host.Port == developmentPort || x.Request.Host.Equals(domainName), subapp =>
        {
            var subpath = $"/{staticWebAssetBasePath}";
            subapp.Use((ctx, nxt) =>
            {
                ctx.Request.Path = $"{subpath}{ctx.Request.Path}";
                return nxt();
            });

            subapp.UseBlazorFrameworkFiles(subpath);
            subapp.UseStaticFiles();
            subapp.UseStaticFiles(subpath);
            subapp.UseRouting();

            subapp.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile(subpath + "/{*path:nonfile}", subpath + "/index.html");
            });

            if (app.Environment.IsDevelopment())
            {
                subapp.UseDeveloperExceptionPage();
                subapp.UseWebAssemblyDebugging();
            }
        });
    }

    public static WebApplication Blossom<TUser>(this WebApplicationBuilder builder, Action<IServiceCollection> services) where TUser : BlossomUser, new()
    {
        builder.AddBlossom();
        services(builder.Services);
        builder.AddBlossomAuthentication<TUser>();

        WebApplication app = builder.Build();
        app.UseBlossom();

        if (builder.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseBlossomAuthentication<TUser>();

        return app;
    }
}
