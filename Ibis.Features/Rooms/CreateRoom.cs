using System.Security.Claims;

namespace Ibis.Features.Rooms;

public partial class Rooms
{
    public async Task<GetRoomResponse> CreateRoomAsync(string roomName, string roomType, List<string> emails, ClaimsPrincipal user, InviteUser inviteUser)
    {
        Room room = new(roomName, roomType, user.Id());

        var existingRoom = Repository.Query.FirstOrDefault(x => x.HostUserId == room.HostUserId && x.Slug == room.Slug);
        if (existingRoom != null)
            throw new ForbiddenException($"A room already exists in your account with the name '{room.Slug}'. Please choose a different name.");

        await Repository.AddAsync(room);
        
        //find current users
        foreach (string email in emails)
            await inviteUser.ExecuteAsync(new(email, room.Id));

        return new(room);
    }
}
