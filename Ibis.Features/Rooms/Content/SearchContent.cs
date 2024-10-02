using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

public record SearchContentRequest(string RoomSlug, string? Tag = null);
public record SearchContentResponse(string Name, string RoomSlug, Dictionary<string, Message> Content);

public class SearchContent
{
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }

    public SearchContent(IRepository<Message> messages, IRepository<Room> rooms)
    {
        Messages = messages;
        Rooms = rooms;
    }

    public async Task<SearchContentResponse> ExecuteAsync(SearchContentRequest request)
    {
        var room = await GetRoomAsync(request.RoomSlug);

        var content = await GetMessagesAsync(request, room);

        var response = new SearchContentResponse(room.Name, room.Slug, content);

        return response;
    }

    private async Task<Room> GetRoomAsync(string slug)
    {
        var room =  Rooms.Query.FirstOrDefault(x => x.Name == slug)
                    ?? Rooms.Query.FirstOrDefault(x => x.Slug == slug)
                    ?? throw new Exception("Room not found.");

        return room;
    }

    private async Task<Dictionary<string, Message>> GetMessagesAsync(SearchContentRequest request, Room room)
    {
        var content = await Messages.Query(room.RoomId)
            .Where(x => x.Text != null)  
            .OrderBy(y => y.Timestamp)   
            .ToDictionaryAsync(          
                message => message.Tag!,  
                message => message        
            );

        return content;
    }
}