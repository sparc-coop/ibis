using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features.Messages;
public record GetAllSlackMessagesRequest(string RoomId, string? Language, string? ContentType = "Text");
public class GetAllSlackMessages : PublicFeature<GetAllSlackMessagesRequest, List<string>>
{
    public GetAllSlackMessages(IRepository<Message> messages)
    {
        Messages = messages;
    }
    public IRepository<Message> Messages { get; }

    public override async Task<List<string>> ExecuteAsync(GetAllSlackMessagesRequest request)
    {
        List<Message> postList = await Messages.Query.Where(x => x.SiteName == request.RoomId).OrderByDescending(y => y.Timestamp).ToListAsync();
        List<string> textList = new List<string>();
        foreach(var item in postList)
        {
            if (request.ContentType.ToLower() == "audio")
            {
                textList.Add(item.Audio.Url);
            } else
            {
                textList.Add(item.Text);
            }          
        }

        return textList;
    }
}
