using Microsoft.EntityFrameworkCore;

namespace Ibis.Features.Messages;

public record GetContentRequest(string RoomId, string Tag, string? Language, string? ContentType = "Text");
public class GetContent : PublicFeature<GetContentRequest, GetContentResponse>
{
    public GetContent(IRepository<Message> messages)
    {
        Messages = messages;
    }
    public IRepository<Message> Messages { get; }

    public override async Task<GetContentResponse> ExecuteAsync(GetContentRequest request)
    {
        var message = await Messages.Query
            .Where(x => x.SiteName == request.RoomId && x.Tag == request.Tag)
            .OrderByDescending(y => y.Timestamp)
            .FirstOrDefaultAsync();

        if (message == null)
            throw new NotFoundException($"Message {request.Tag} not found!");

        return new(message.Tag!, message.Text!, message.Audio?.Url);
    }
}
