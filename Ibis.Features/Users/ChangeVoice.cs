using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Users;

public record ChangeVoiceRequest(string Language, string VoiceName, string? RoomId);
public class ChangeVoice : Feature<ChangeVoiceRequest, bool>
{
    public ChangeVoice(IRepository<User> users, IRepository<Room> rooms, ITranslator translator, ISpeaker speaker)
    {
        Users = users;
        Rooms = rooms;
        Translator = translator;
        Speaker = speaker;
    }

    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }
    public ISpeaker Speaker { get; }

    public override async Task<bool> ExecuteAsync(ChangeVoiceRequest request)
    {
        var language = await Translator.GetLanguageAsync(request.Language);
        if (language == null)
            throw new Exception("Language not found!");

        var voices = await Speaker.GetVoicesAsync(request.Language);
        var voice = voices.FirstOrDefault(x => x.ShortName == request.VoiceName);
        if (voice == null)
            throw new Exception("Voice doesn't match language!");
        
        await Users.ExecuteAsync(User.Id(), x => x.ChangeVoice(language, voice));

        if (request.RoomId != null)
            await Rooms.ExecuteAsync(request.RoomId, x => x.AddLanguage(language));

        return true;
    }
}
