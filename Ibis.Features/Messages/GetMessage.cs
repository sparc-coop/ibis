using Microsoft.EntityFrameworkCore;

namespace Ibis.Features.Messages;

public record GetMessageRequest(string RoomSlug, string Tag, string? Language = null, bool AsHtml = false);
public class GetMessage : PublicFeature<GetMessageRequest, Message>
{
    public GetMessage(IRepository<Message> messages, IRepository<Room> rooms)
    {
        Messages = messages;
        Rooms = rooms;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }

    public override async Task<Message> ExecuteAsync(GetMessageRequest request)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug)
            ?? throw new NotFoundException($"Room {request.RoomSlug} not found");

        var query = Messages.Query
            .Where(x => x.RoomId == room.Id && x.Tag == request.Tag);

        query = request.Language == null
            ? query.Where(x => x.SourceMessageId == null)
            : query.Where(x => x.Language == request.Language);

        var message = await query
            .OrderByDescending(y => y.Timestamp)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Message {request.Tag} not found!");

        return message;
    }
}
