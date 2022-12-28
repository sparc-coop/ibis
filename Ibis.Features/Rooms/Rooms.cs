namespace Ibis.Features.Rooms;

public partial class Rooms : BlossomAggregate<Room>
{
    public IRepository<Message> Messages { get; }

    public Rooms(IRepository<Room> rooms, IRepository<Message> messages, IHttpContextAccessor http) : base(rooms, http)
    {
        Messages = messages;

        GetAllAsync = () => new GetRooms(User);
        CreateAsync = CreateRoomAsync;
        DeleteAsync = async (string id) => await Repository.ExecuteAsync(id, room => room.Close());
        UpdateAsync = UpdateRoomAsync;
    }

    public override void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        base.MapEndpoints(endpoints);

        RootEndpoints.MapPost("Join", JoinRoomAsync);
        RootEndpoints.MapPost("Leave", LeaveRoomAsync);
    }

    async Task<Room?> JoinRoomAsync(string id, IRepository<User> Users)
    {
        var room = await Repository.FindAsync(id);
        var user = await Users.GetAsync(User);
        if (room == null || user == null)
            return null;

        await Repository.ExecuteAsync(room.Id, room => room.AddActiveUser(user));

        return room;
    }

    public async Task<bool> LeaveRoomAsync(string id, IRepository<User> Users)
    {
        var room = await Repository.FindAsync(id);
        var user = await Users.GetAsync(User);
        if (room == null || user == null)
            throw new NotFoundException($"Room {id} not found!");

        await Users.ExecuteAsync(User.Id(), user => user.LeaveRoom(id));
        await Repository.ExecuteAsync(id, room => room.RemoveActiveUser(user));

        return true;
    }

    async Task<bool> UpdateRoomAsync(string id, string title)
    {
        var room = await Repository.FindAsync(id);
        if (room == null)
            return false;

        room.SetName(title);
        await Repository.UpdateAsync(room);
        return true;
    }
}
