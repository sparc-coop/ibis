namespace Ibis.Features.Messages;
public record GetAllContentRequest(string RoomId, string Language);
public record GetContentResponse(string Tag, string Text, string? Audio);
public class GetAllContent : PublicFeature<GetAllContentRequest, List<GetContentResponse>>
{
    public GetAllContent(IRepository<Message> messages)
    {
        Messages = messages;
    }
    public IRepository<Message> Messages { get; }

    public override async Task<List<GetContentResponse>> ExecuteAsync(GetAllContentRequest request)
    {
        List<Message> postList = await Messages.Query
            .Where(x => x.RoomId == request.RoomId && x.Language == request.Language && x.Text != null)
            .OrderByDescending(y => y.Timestamp)
            .ToListAsync();

        List<GetContentResponse> result = new();
        foreach (var item in postList)
        {
            result.Add(new(item.Tag ?? item.Id, item.Text!, item.Audio?.Url));
        }

        return result;
    }
}
