namespace Ibis.Features.Users;

public record ChangeLanguageRequest(string Language, string? RoomId);
public class ChangeLanguage : Feature<ChangeLanguageRequest, bool>
{
    public ChangeLanguage(IRepository<User> users, IRepository<Room> rooms, ITranslator translator)
    {
        Users = users;
        Rooms = rooms;
        Translator = translator;
    }

    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }

    public override async Task<bool> ExecuteAsync(ChangeLanguageRequest request)
    {
        var language = await Translator.GetLanguageAsync(request.Language);
        if (language == null)
            throw new Exception("Language not found!");
        
        await Users.ExecuteAsync(User.Id(), x => x.ChangeLanguage(language));

        if (request.RoomId != null)
            await Rooms.ExecuteAsync(request.RoomId, x => x.AddLanguage(language));

        return true;
    }
}
