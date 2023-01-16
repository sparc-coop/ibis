namespace Ibis.Messages;

public record GetAllMessagesRequest(string RoomId);
public record GetAllMessagesResponse(string Language, List<Message> Messages);
public class GetAllMessages : Feature<GetAllMessagesRequest, GetAllMessagesResponse>
{
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }

    public GetAllMessages(IRepository<Message> messages, IRepository<User> users)
    {
        Messages = messages;
        Users = users;
    }

    public async override Task<GetAllMessagesResponse> ExecuteAsync(GetAllMessagesRequest request)
    {
        var user = await Users.GetAsync(User);
        if (user == null || user.Avatar.Language == null)
            throw new NotFoundException($"User {User.Id()} not found!");

        var messages = await Messages
                .Query
                .Where(message => message.RoomId == request.RoomId && message.Language == user!.Avatar.Language)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

        return new(user!.Avatar.Language, messages);
    }
}
