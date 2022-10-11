using Microsoft.EntityFrameworkCore;

namespace Ibis.Features.Messages;

public record GetContentRequest(string RoomSlug, string Tag, string Language);
public class GetContent : PublicFeature<GetContentRequest, GetContentResponse>
{
    public GetContent(IRepository<Message> messages, IRepository<Room> rooms)
    {
        Messages = messages;
        Rooms = rooms;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }

    public override async Task<GetContentResponse> ExecuteAsync(GetContentRequest request)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug);
        if (room == null)
        {
            throw new NotFoundException($"Room {request.RoomSlug} not found");
        }

        var message = await Messages.Query
            .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Tag == request.Tag)
            .OrderByDescending(y => y.Timestamp)
            .FirstOrDefaultAsync();

        if (message == null)
            throw new NotFoundException($"Message {request.Tag} not found!");

        return new(message.Tag!, message.Text!, message.Audio?.Url);
    }
}
