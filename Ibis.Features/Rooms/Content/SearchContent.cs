using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

public record SearchContentRequest(string SearchTerm);
public record SearchContentResponse(string MessageId, string MessageRoomId, string MessageRoomName, string MessageText, string MessageTag);

public class SearchContent
{
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }

    public SearchContent(IRepository<Message> messages, IRepository<Room> rooms)
    {
        Messages = messages;
        Rooms = rooms;
    }

    public async Task<List<SearchContentResponse>> ExecuteAsync(SearchContentRequest request)
    {
        var searchTerm = request.SearchTerm.ToLower();

        var matchingMessages = await GetMessagesAsync(searchTerm);

        return matchingMessages;
    }

    private async Task<List<SearchContentResponse>> GetMessagesAsync(string searchTerm)
    {
        var messages = await Messages.Query
            .Where(x => (x.Text != null && x.Text.ToLower().Contains(searchTerm)) ||
                        (x.Tag != null && x.Tag.ToLower().Contains(searchTerm)))
            .Select(x => new { x.Id, x.RoomId, x.Text, x.Tag })
            .ToListAsync();

        var roomIds = messages.Select(m => m.RoomId).Distinct().ToList();

        var rooms = await Rooms.Query
            .Where(r => roomIds.Contains(r.Id))
            .ToListAsync();

        return messages.Select(message =>
        {
            var room = rooms.FirstOrDefault(r => r.Id == message.RoomId);
            return new SearchContentResponse(
                message.Id,
                message.RoomId,
                room?.Name,
                message.Text,
                message.Tag
            );
        }).ToList();
    }
}