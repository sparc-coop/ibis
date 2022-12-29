namespace Ibis.Features.Rooms;

public partial class Rooms : BlossomAggregate<Room>
{
    public Rooms() : base()
    {
        //GetAllAsync = () => new GetRooms(User);
        CreateAsync = CreateRoomAsync;
        DeleteAsync = (Room room) => room.Close();
    }

    public override void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        base.MapEndpoints(endpoints);

        RootEndpoints.MapPut("Join", (Room room, User user) => room.AddActiveUser(user));
        RootEndpoints.MapPut("AddLanguage", (Room room, string id, string displayName, string nativeName, bool rightToLeft) => room.AddLanguage(new(id, displayName, nativeName, rightToLeft)));
    }
}
