using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features.Messages;

public record GetSlackMessageRequest(string RoomId, string Tag, string? Language, string? ContentType = "Text");
public class GetSlackPosts : PublicFeature<GetSlackMessageRequest, string>
{
    public GetSlackPosts(IRepository<Message> messages)
    {
        Messages = messages;
    }
    public IRepository<Message> Messages { get; }

    public override async Task<string> ExecuteAsync(GetSlackMessageRequest request)
    {
        Message message = Messages.Query.Where(x => x.Tag == request.id).OrderByDescending(y => y.Timestamp).FirstOrDefault();
        return message.Text;
    }
}
