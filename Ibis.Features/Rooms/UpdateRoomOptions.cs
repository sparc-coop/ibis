﻿namespace Ibis;

public record RoomOptionsRequest(string roomId, string Title);
public class UpdateRoomOptions : Feature<RoomOptionsRequest, bool>
{ 
    public UpdateRoomOptions(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }
    public IRepository<Room> Rooms { get; }


    public override async Task<bool> ExecuteAsync(RoomOptionsRequest request)
    {
        var room = await Rooms.FindAsync(request.roomId);
        if (room == null)
            return false;

        room.SetName(request.Title);
        await Rooms.UpdateAsync(room);
        return true;
    }
}