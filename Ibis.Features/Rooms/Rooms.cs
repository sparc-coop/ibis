using Ardalis.Specification;

namespace Ibis.Features.Rooms;

public partial class Rooms : BlossomModule<Room, string, GetRoomResponse>
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
        MapPost("Join", room => room.AddActiveUser(User));
    }

    protected override ISpecification<Room> GetAllAsync() => new GetRooms(User);

    protected override async Task DeleteAsync(Room result) => result.Close();

    protected override GetRoomResponse Map(Room entity) => new(entity);

    protected override Room Map(GetRoomResponse entity) => new(entity.Name, entity.RoomType, string.Empty);
}
