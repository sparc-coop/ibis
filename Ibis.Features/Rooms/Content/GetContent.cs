using Microsoft.EntityFrameworkCore;

namespace Ibis.Features.Rooms;

public record GetContentRequest(string RoomSlug, string Tag, string Language, bool AsHtml = false);
public class GetContent : PublicFeature<GetContentRequest, Message>
{
    public GetContent(IRepository<Message> messages, IRepository<Room> rooms)
    {
        Messages = messages;
        Rooms = rooms;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }

    public override async Task<Message> ExecuteAsync(GetContentRequest request)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug) 
            ?? throw new NotFoundException($"Room {request.RoomSlug} not found");
        
        var message = await Messages.Query
            .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Tag == request.Tag)
            .OrderByDescending(y => y.Timestamp)
            .FirstOrDefaultAsync() 
            ?? throw new NotFoundException($"Message {request.Tag} not found!");
        
        return message;
    }
}
