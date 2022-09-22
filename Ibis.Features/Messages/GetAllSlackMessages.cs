using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features.Messages;
public record GetAllSlackMessagesRequest(string RoomId, string? Language, string? ContentType = "Text");
public class GetAllSlackMessages : PublicFeature<GetAllSlackMessagesRequest, List<string>>
{
    public GetAllSlackMessages(IRepository<Message> posts)
    {
        Posts = posts;
    }
    public IRepository<Message> Posts { get; }

    public override async Task<List<string>> ExecuteAsync(GetAllSlackMessagesRequest request)
    {
        List<Message> postList = await Posts.Query.Where(x => x.SiteName == request.RoomId).OrderByDescending(y => y.Timestamp).ToListAsync();
        List<string> textList = new List<string>();
        foreach(var item in postList)
        {
            textList.Add(item.Text);
        }

        return textList;
    }
}
