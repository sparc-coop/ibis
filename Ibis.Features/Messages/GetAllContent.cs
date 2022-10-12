namespace Ibis.Features.Messages;
public record GetAllContentRequest(string RoomSlug, string Language);
public record GetAllContentResponse(string Name, string Slug, string Language, List<GetContentResponse> Content);
public record GetContentResponse(string Tag, string Text, string? Audio, DateTime Timestamp);
public class GetAllContent : PublicFeature<GetAllContentRequest, GetAllContentResponse>
{
    public GetAllContent(IRepository<Message> messages, IRepository<Room> rooms)
    {
        Messages = messages;
        Rooms = rooms;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }

    public override async Task<GetAllContentResponse> ExecuteAsync(GetAllContentRequest request)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug);
        if (room == null)
        {
            throw new NotFoundException($"Room {request.RoomSlug} not found");
        }

        List<Message> postList = await Messages.Query
            .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Text != null)
            .OrderByDescending(y => y.Timestamp)
            .ToListAsync();

        List<GetContentResponse> result = new();
        foreach (var item in postList)
        {
            result.Add(new(item.Tag ?? item.Id, item.Text!, item.Audio?.Url, item.Timestamp));
        }

        return new(room.Name, room.Slug, request.Language, result);
    }
}
