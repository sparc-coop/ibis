﻿using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Ibis.Features.Sparc.Realtime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSparcRealtime<THub>(this IServiceCollection services, string? azureSignalRConnectionString = null) where THub : SparcHub
    {
        services.AddSwaggerGen(options =>
        {
            options.DocumentFilter<PolymorphismDocumentFilter<SparcNotification>>();
            options.SchemaFilter<PolymorphismSchemaFilter<SparcNotification>>();
        });

        var signalR = services.AddSignalR()
            .AddMessagePackProtocol();

        if (azureSignalRConnectionString != null)
            signalR.AddAzureSignalR(azureSignalRConnectionString);

        services.AddSingleton<Publisher>();

        // Use the User ID as the SignalR user identifier    
        services.AddSingleton<IUserIdProvider, UserIdProvider>();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(_ => new SparcHubAuthenticator("hub"));

        // services.AddMediatR(typeof(THub));
        services.AddTransient<IHubContext<SparcHub>>(s => s.GetRequiredService<IHubContext<THub>>());

        // Manually register event handlers to avoid bug https://github.com/jbogard/MediatR/issues/718
        //var types = typeof(THub).Assembly.GetTypes();
        //var notifications = types.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(SparcNotification)));
        //foreach (var evt in notifications)
        //{
        //    services.AddTransient(typeof(INotificationHandler<>).MakeGenericType(evt), typeof(SparcNotificationForwarder<>).MakeGenericType(evt));
        //    foreach (var handler in types.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(RealtimeFeature<>).MakeGenericType(evt))))
        //    {
        //        services.AddTransient(typeof(INotificationHandler<>).MakeGenericType(evt), handler);
        //    }
        //}

        return services;
    }
}

// taken from https://stackoverflow.com/questions/49006079/using-swashbuckle-for-asp-net-core-how-can-i-add-a-model-to-the-generated-model
public class PolymorphismDocumentFilter<T> : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
    {
        RegisterSubClasses(context, typeof(T));
    }

    private static void RegisterSubClasses(DocumentFilterContext context, Type abstractType)
    {
        const string discriminatorName = "$type";
        var schemaRepository = context.SchemaRepository.Schemas;
        var schemaGenerator = context.SchemaGenerator;

        if (!schemaRepository.TryGetValue(abstractType.Name, out OpenApiSchema? parentSchema))
        {
            parentSchema = schemaGenerator.GenerateSchema(abstractType, context.SchemaRepository);
        }

        // set up a discriminator property (it must be required)
        parentSchema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
        parentSchema.Required.Add(discriminatorName);

        if (!parentSchema.Properties.ContainsKey(discriminatorName))
            parentSchema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string", Default = new OpenApiString(abstractType.FullName) });

        // register all subclasses
        var derivedTypes = abstractType.GetTypeInfo().Assembly.GetTypes()
            .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

        foreach (var type in derivedTypes)
            schemaGenerator.GenerateSchema(type, context.SchemaRepository);
    }
}

public class PolymorphismSchemaFilter<T> : ISchemaFilter
{
    private readonly Lazy<HashSet<Type>> derivedTypes = new(Init);

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!derivedTypes.Value.Contains(type))
            return;

        var baseProperties = typeof(T).GetProperties().Select(x => x.Name).ToList();
        var clonedProperties = schema.Properties
            .Where(x => !baseProperties.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key, x => x.Value);

        var clonedSchema = new OpenApiSchema
        {
            Properties = clonedProperties,
            Type = schema.Type,
            Required = schema.Required
        };

        if (context.SchemaRepository.Schemas.TryGetValue(typeof(T).Name, out OpenApiSchema _))
        {
            schema.AllOf = new List<OpenApiSchema> {
            new OpenApiSchema { Reference = new OpenApiReference { Id = typeof(T).Name, Type = ReferenceType.Schema } },
            clonedSchema
        };
        }

        var assemblyName = Assembly.GetAssembly(type)!.GetName();
        schema.Discriminator = new OpenApiDiscriminator { PropertyName = "$type" };
        schema.AddExtension("x-ms-discriminator-value", new OpenApiString($"{type.FullName}, {assemblyName.Name}"));

        // reset properties for they are included in allOf, should be null but code does not handle it
        schema.Properties = new Dictionary<string, OpenApiSchema>();
    }

    private static HashSet<Type> Init()
    {
        var abstractType = typeof(T);
        var dTypes = abstractType.GetTypeInfo().Assembly
            .GetTypes()
            .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

        var result = new HashSet<Type>();

        foreach (var item in dTypes)
            result.Add(item);

        return result;
    }
}
