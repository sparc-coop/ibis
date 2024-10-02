using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

public record SearchContentRequest(string SearchTerm);
public record SearchContentResponse(List<Room> Rooms, List<Message> Messages);

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
        var searchTerm = request.SearchTerm;

        var matchingRooms = await GetRoomBySlugAsync(searchTerm);

        var matchingMessages = await GetMessagesInAllRoomsAsync(searchTerm);

        return new SearchContentResponse(matchingRooms, matchingMessages);
    }

    private async Task<List<Room>> GetRoomBySlugAsync(string searchTerm)
    {
        return await Rooms.Query
            .Where(x => x.Slug.Contains(searchTerm))
            .ToListAsync();
    }

    private async Task<List<Message>> GetMessagesInAllRoomsAsync(string searchTerm)
    {
        return await Messages.Query
            .Where(x => (x.Text != null && x.Text.Contains(searchTerm)) ||
                        (x.Tag != null && x.Tag.Contains(searchTerm)))
            .OrderBy(y => y.Timestamp)
            .ToListAsync();
    }
}