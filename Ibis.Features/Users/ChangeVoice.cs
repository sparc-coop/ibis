using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Users;

public record ChangeVoiceRequest(string Language, string VoiceName, string? RoomId);
public record ChangeVoiceResponse(string? PreviewAudioUrl);
public class ChangeVoice : Feature<ChangeVoiceRequest, ChangeVoiceResponse>
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

    public override async Task<ChangeVoiceResponse> ExecuteAsync(ChangeVoiceRequest request)
    {
        var language = await Translator.GetLanguageAsync(request.Language);
        if (language == null)
            throw new Exception("Language not found!");

        var voices = await Speaker.GetVoicesAsync(request.Language);
        var voice = voices.FirstOrDefault(x => x.ShortName == request.VoiceName);
        if (voice == null)
            throw new Exception("Voice doesn't match language!");

        var user = await Users.GetAsync(User);
        user!.ChangeVoice(language, voice);
        await Users.UpdateAsync(user);

        if (request.RoomId != null)
            await Rooms.ExecuteAsync(request.RoomId, x => x.AddLanguage(language));

        var name = string.IsNullOrWhiteSpace(user.Avatar.Name) ? voice.DisplayName : user.Avatar.Name;
        var testMessage = new Message("", user, $"Hi, nice to meet you!");

        var translation = await Translator.TranslateAsync(testMessage, "en", new() { language });
        var speech = await Speaker.SpeakAsync(translation.First());
        return new(speech?.Url);
    }
}
