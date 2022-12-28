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
    }

    public IRepository<T> Repository { get; }
    public ClaimsPrincipal User => Http?.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    protected RouteGroupBuilder Endpoints = null!;
    protected string BaseUrl => $"/{Name.ToLower()}";
    IHttpContextAccessor Http { get; }
    public string Name { get; set; }

    protected virtual void OnModelCreating(IEndpointRouteBuilder endpoints)
    {
        MapBaseEndpoints(endpoints);
    }

    protected void MapBaseEndpoints(IEndpointRouteBuilder endpoints)
    {
        Endpoints = endpoints.MapGroup(BaseUrl)
            .WithGroupName(Name)
            .WithOpenApi();

        MapGet("", InternalGetAllAsync);
        MapGet("/{id}", InternalGetAsync);
        MapPost("", InternalCreateAsync);
        MapPut("/{id}", InternalUpdateAsync);
        MapDelete("/{id}", InternalDeleteAsync);

        foreach (var method in typeof(T).GetMethods())
            MapPost("/{id}/" + method.Name, CreateDelegate(method));
    }

    protected async Task<Ok<List<T>>> InternalGetAllAsync()
    {
        var results = await Repository.GetAllAsync(GetAllAsync());
        return TypedResults.Ok(results);
    }

    protected async Task<Results<NotFound, Ok<T>>> InternalGetAsync(string id)
    {
        var result = await GetAsync(id);
        return result == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    protected async Task<Created<T>> InternalCreateAsync(T entity)
    {
        await Repository.AddAsync(entity);
        return TypedResults.Created($"{BaseUrl}/{entity.Id}", entity);
    }

    protected async Task<Results<NotFound, Ok<T>>> InternalUpdateAsync(string id, T entity)
    {
        entity.Id = id;
        await Repository.UpdateAsync(entity);
        return TypedResults.Ok(entity);
    }

    protected async Task<Results<NotFound, NoContent>> InternalDeleteAsync(string id)
    {
        var result = await Repository.FindAsync(id);
        if (result == null)
            return TypedResults.NotFound();

        await DeleteAsync(result);
        return TypedResults.NoContent();
    }

    protected virtual async Task<T> CreateAsync(T entity)
    {
        await Repository.AddAsync(entity);
        return entity;
    }

    protected virtual async Task<T?> GetAsync(object id) => await Repository.FindAsync(id);
    protected abstract ISpecification<T> GetAllAsync();
    
    protected virtual async Task DeleteAsync(T result) => await Repository.DeleteAsync(result);

    protected void MapGet(string path, Delegate action) => Endpoints.MapGet(path, action);
    protected void MapPost(string path, Delegate action) => Endpoints.MapPost(path, action);
    protected void MapPut(string path, Delegate action) => Endpoints.MapPut(path, action);
    protected void MapDelete(string path, Delegate action) => Endpoints.MapDelete(path, action);
    protected void MapPost(string path, Action<T> action)
    {
        Endpoints.MapPost("/{id}/" + path, async (string id) =>
        {
            var room = await GetAsync(id);
            if (room == null)
                return Results.NotFound();

            await Repository.ExecuteAsync(room, action);
            return Results.Ok();
        });
    }

    static Delegate CreateDelegate(MethodInfo method)
    {
        if (method == null)
        {
            throw new ArgumentNullException("method");
        }

        if (!method.IsStatic)
        {
            throw new ArgumentException("The provided method must be static.", "method");
        }

        if (method.IsGenericMethod)
        {
            throw new ArgumentException("The provided method must not be generic.", "method");
        }

        return method.CreateDelegate(Expression.GetDelegateType(
            (from parameter in method.GetParameters() select parameter.ParameterType)
            .Concat(new[] { method.ReturnType })
            .ToArray()));
    }
}
