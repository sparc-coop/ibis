using Ardalis.Specification;
using System.Security.Claims;

namespace Sparc.Blossom;

public interface IBlossomAggregate
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints);
}

public abstract class BlossomAggregate<T> : IBlossomAggregate where T : Root<string>
{
    public BlossomAggregate(IRepository<T> repository, IHttpContextAccessor http)
    {
        Repository = repository;
        Http = http;
        GetAllAsync = () => new BlossomGetAllSpecification<T>(100);
    }

    public IRepository<T> Repository { get; }
    public ClaimsPrincipal User => Http?.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    protected RouteGroupBuilder AggregateEndpoints = null!;
    protected RouteGroupBuilder RootEndpoints = null!;
    protected string BaseUrl => $"/{Name.ToLower()}";
    IHttpContextAccessor Http { get; }
    public virtual string Name => typeof(T).Name + "s";

    public virtual void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        MapBaseEndpoints(endpoints);
    }

    protected void MapBaseEndpoints(IEndpointRouteBuilder endpoints)
    {
        AggregateEndpoints = endpoints.MapGroup(BaseUrl)
            .WithGroupName(Name)
            .WithOpenApi();

        RootEndpoints = AggregateEndpoints.MapGroup("{id}");

        AggregateEndpoints.MapGet("", DefaultGetAllAsync);
        AggregateEndpoints.MapPost("", CreateAsync ?? DefaultCreateAsync);
        RootEndpoints.MapGet("", DefaultGetAsync);
        RootEndpoints.MapPut("", UpdateAsync ?? DefaultUpdateAsync);
        RootEndpoints.MapDelete("", DeleteAsync ?? DefaultDeleteAsync);
    }

    protected async Task<Ok<List<T>>> DefaultGetAllAsync()
    {
        var results = await Repository.GetAllAsync(GetAllAsync());
        return TypedResults.Ok(results);
    }

    protected async Task<Results<NotFound, Ok<T>>> DefaultGetAsync(string id)
    {
        var result = await Repository.FindAsync(id);
        return result == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    protected async Task<Created<T>> DefaultCreateAsync(T entity)
    {
        await Repository.AddAsync(entity);
        return TypedResults.Created($"{BaseUrl}/{entity.Id}", entity);
    }

    protected async Task<Results<NotFound, Ok<T>>> DefaultUpdateAsync(string id, T entity)
    {
        entity.Id = id;
        await Repository.UpdateAsync(entity);
        return TypedResults.Ok(entity);
    }

    protected async Task<Results<NotFound, NoContent>> DefaultDeleteAsync(string id)
    {
        var result = await Repository.FindAsync(id);
        if (result == null)
            return TypedResults.NotFound();

        if (DeleteAsync != null)
            await DeleteAsync.InvokeAsync<T>(result);
        else
            await Repository.DeleteAsync(result);

        return TypedResults.NoContent();
    }

    protected Delegate? GetAsync;
    protected Func<ISpecification<T>> GetAllAsync;
    protected Delegate? CreateAsync;
    protected Delegate? UpdateAsync;
    protected Delegate? DeleteAsync;
}

public static class BlossomAggregateExtensions
{
    public static async Task<T> InvokeAsync<T>(this Delegate method, params object?[]? args) where T : class
    {
        var result = method.DynamicInvoke(args);
        if (result is Task<T> task) return await task;
        return result as T ?? throw new InvalidOperationException("Invalid return type");
    }

    public static IServiceCollection RegisterAggregates(this IServiceCollection services)
    {
        var modules = DiscoverAggregates();
        foreach (var module in modules)
            services.AddScoped(module);

        return services;
    }

    private static IEnumerable<Type> DiscoverAggregates()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var aggregates = assemblies.Distinct().SelectMany(x => x.GetTypes())
            .Where(x => typeof(IBlossomAggregate).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        return aggregates;
    }

    public static void MapAggregates(this WebApplication app)
    {
        var aggregates = DiscoverAggregates();
        foreach (var aggregate in aggregates)
        {
            var instance = app.Services.GetRequiredService(aggregate) as IBlossomAggregate;
            instance?.MapEndpoints(app);
        }
    }
}