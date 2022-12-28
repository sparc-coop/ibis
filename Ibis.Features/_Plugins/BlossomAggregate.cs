using Ardalis.Specification;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace Sparc.Blossom;

public abstract class BlossomAggregate<T> where T : Root<string>
{
    public BlossomAggregate(IRepository<T> repository, IHttpContextAccessor http, string? baseUrl = null)
    {
        Repository = repository;
        Http = http;
        Name = baseUrl?.Trim('/') ?? (typeof(T).Name + "s");
        
        GetAllAsync = () => new BlossomGetAllSpecification<T>(100);
    }

    public IRepository<T> Repository { get; }
    public ClaimsPrincipal User => Http?.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    protected RouteGroupBuilder AggregateEndpoints = null!;
    protected RouteGroupBuilder RootEndpoints = null!;
    protected string BaseUrl => $"/{Name.ToLower()}";
    IHttpContextAccessor Http { get; }
    public string Name { get; set; }

    protected virtual void OnModelCreating(IEndpointRouteBuilder endpoints)
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

        foreach (var method in typeof(T).GetMethods())
            MapCommand(method.Name, method);
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

    protected void MapCommand(MethodInfo method)
    {
        RootEndpoints.MapPost(method.Name, async (string id, HttpContext ) =>
        {
            var entity = await Repository.FindAsync(id);
            if (entity == null)
                return Results.NotFound();

            method.Invoke(entity, );
            await Repository.ExecuteAsync(entity, action);
            return Results.Ok();
        });
    }

    static Delegate CreateDelegate(MethodInfo method)
    {
        return method.CreateDelegate(Expression.GetDelegateType(
            (from parameter in method.GetParameters() select parameter.ParameterType)
            .Concat(new[] { method.ReturnType })
            .ToArray()));
    }
}

public static class BlossomAggregateExtensions
{
    public static async Task<T> InvokeAsync<T>(this Delegate method, params object?[]? args) where T : class
    {
        var result = method.DynamicInvoke(args);
        if (result is Task<T> task) return await task;
        return result as T ?? throw new InvalidOperationException("Invalid return type");
    }
}