using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Rooms;

public class RoomHub : Hub
{
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }

    public RoomHub(IRepository<User> users, IRepository<Room> rooms)
    {
        Users = users;
        Rooms = rooms;
    }

    public async Task SendMessage(string roomId, Message message)
    {
        await Clients.Group(roomId).SendAsync("NewMessage", message);
    }

    public async Task AddToRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        //await Groups.AddToGroupAsync(Context.ConnectionId, $"{roomId}|{language}");
    }

    public async Task RemoveFromRoom(string roomId, string userId)
    {
        roomId = await UnregisterRoomAsync(roomId, userId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        //await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{roomId}|{language}");
    }

    private async Task<string> UnregisterRoomAsync(string roomId, string userId)
    {
        var user = await Users.FindAsync(userId);
        roomId = user!.LeaveRoom(roomId) ?? roomId;
        await Users.UpdateAsync(user);

        await Rooms.ExecuteAsync(roomId, room => room.RemoveActiveUser(user));

        return roomId;
    }

    public async Task UpdateRoom(Room room)
    {
        await Rooms.UpdateAsync(room);
    }
}
