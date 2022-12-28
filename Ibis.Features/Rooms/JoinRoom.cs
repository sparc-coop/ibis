namespace Ibis.Features.Rooms;

public partial class Rooms
{
    public async Task<Room?> JoinAsync(string id, IRepository<User> Users)
    {
        var room = await GetAsync(id);
        var user = await Users.GetAsync(User);
        if (room == null || user == null)
            return null;

        await Repository.ExecuteAsync(room.Id, room => room.AddActiveUser(user));
        
        return room;
    }
}
