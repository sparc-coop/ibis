namespace Ibis.Features.Users;

public record ChangeLanguageRequest(string Language, string? RoomId);
public class ChangeLanguage : Feature<ChangeLanguageRequest, bool>
{
    public ChangeLanguage(IRepository<User> users, IRepository<Room> rooms)
    {
        Users = users;
        Rooms = rooms;
    }

    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }

    public override async Task<bool> ExecuteAsync(ChangeLanguageRequest request)
    {
        await Users.ExecuteAsync(User.Id(), x => x.ChangeLanguage(request.Language));

        if (request.RoomId != null)
            await Rooms.ExecuteAsync(request.RoomId, x => x.AddLanguage(request.Language));

        return true;
    }
}
