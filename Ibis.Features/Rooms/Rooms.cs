using Ardalis.Specification;

namespace Ibis.Features.Rooms;

public partial class Rooms : BlossomAggregate<Room>
{
    public IRepository<Message> Messages { get; }

    public Rooms(IRepository<Room> rooms, IRepository<Message> messages, IHttpContextAccessor http) : base(rooms, http)
    {
        Messages = messages;
    }

    protected override void OnModelCreating(IEndpointRouteBuilder endpoints)
    {
        base.OnModelCreating(endpoints);

        MapPost("", CreateRoomAsync);
        MapGet("/{id}/Text", GetTextAsync);

    }

    protected override ISpecification<Room> GetAllAsync() => new GetRooms(User);

    protected override Task DeleteAsync(Room result)
    {
        result.Close();
        return Task.CompletedTask;
    }
}
