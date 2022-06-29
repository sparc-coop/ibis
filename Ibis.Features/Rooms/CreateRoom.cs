using Ibis.Features.Users.Entities;

namespace Ibis.Features.Rooms;

public record NewRoomRequest(string RoomName, List<RoomUser>? Participants);
public class CreateRoom : Feature<NewRoomRequest, GetRoomResponse>
{
    public CreateRoom(IRepository<Room> rooms, IRepository<User> users)
    {
        Rooms = rooms;
        Users = users;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public async override Task<GetRoomResponse> ExecuteAsync(NewRoomRequest request)
    {
        var room = new Room(request.RoomName, User.Id());
        await Rooms.AddAsync(room);

        // set room language preset to hostUser default preset
        var user = await Users.FindAsync(User.Id());
        var newPair = new LanguagePresetRoomPair(user.Id.ToString(), user.DefaultLanguagePresetId, room.Id, DateTime.Now);
        user.LanguagePresetsForRooms.Add(newPair);
        await Users.UpdateAsync(user);

        return new(room);
    }
}
