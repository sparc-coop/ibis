using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features.Messages;

public record GetSlackMessageRequest(string RoomId, string Tag, string? Language, string? ContentType = "Text");
public class GetSlackMessage : PublicFeature<GetSlackMessageRequest, string>
{
    public GetSlackMessage(IRepository<Message> messages)
    {
        Messages = messages;
    }
    public IRepository<Message> Messages { get; }

    public override async Task<string> ExecuteAsync(GetSlackMessageRequest request)
    {
        try
        {
            Message message = Messages.Query.Where(x =>
                x.SiteName == request.RoomId
                && x.Tag == request.Tag)
                .OrderByDescending(y => y.Timestamp)
                .FirstOrDefault();

            if(request.ContentType.ToLower() == "audio")
            {
                return message.Audio.Url;
            }

            return message.Text;
        } catch
        {
            return "Oops! This message does not exist";
        }
    }
}
