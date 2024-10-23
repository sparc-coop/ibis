using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ibis.Rooms;

public record SearchContentRequest(string SearchTerm);
public record SearchContentResponse(List<RoomSummary> Rooms, List<MessageSummary> Message);
public record RoomSummary(string Id, string Name, string Slug);
public record MessageSummary(string Id, string RoomId, string Text, string Tag, string RoomName);

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
        var searchTerm = request.SearchTerm.ToLower();

        var matchingRooms = await GetRoomSummariesBySlugAsync(searchTerm);

        var matchingMessages = await GetMessageSummariesInAllRoomsAsync(searchTerm);

        return new SearchContentResponse(matchingRooms, matchingMessages);
    }

    private async Task<List<RoomSummary>> GetRoomSummariesBySlugAsync(string searchTerm)
    {
        return await Rooms.Query
            .Where(x => (x.Slug.ToLower().Contains(searchTerm)) ||
                        (x.Name.ToLower().Contains(searchTerm)))
            .Select(x => new RoomSummary(x.Id, x.Name, x.Slug))
            .ToListAsync();
    }    

    private async Task<List<MessageSummary>> GetMessageSummariesInAllRoomsAsync(string searchTerm)
    {        
        var messages = await Messages.Query
            .Where(x => (x.Text != null && x.Text.ToLower().Contains(searchTerm)) ||
                        (x.Tag != null && x.Tag.ToLower().Contains(searchTerm)))
            .ToListAsync();
        
        var roomIds = messages.Select(m => m.RoomId).Distinct().ToList();
        
        var rooms = await Rooms.Query
            .Where(r => roomIds.Contains(r.Id))
            .ToListAsync();
        
        return messages.Select(message =>
        {
            var room = rooms.FirstOrDefault(r => r.Id == message.RoomId);
            return new MessageSummary(message.Id, message.RoomId, message.Text, message.Tag, room?.Name);
        }).ToList();
    }
}