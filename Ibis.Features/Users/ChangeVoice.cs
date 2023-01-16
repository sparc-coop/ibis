namespace Ibis.Users;

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
        //var testMessage = new Message("", user, $"Hi, nice to meet you!");

        // test messages for voice model preview
        // chooses a random testMessage from this list every time user clicks voice model
        var testMessages = new List<Message>();
        testMessages.Add(new Message("", user, $"Hi, nice to meet you!"));
        testMessages.Add(new Message("", user, $"Hey, how are things going today?"));
        testMessages.Add(new Message("", user, $"What time do you want to meet?"));
        testMessages.Add(new Message("", user, $"Thanks, talk to you later!"));

        int index = new Random().Next(testMessages.Count);
        var testMessage = testMessages[index];

        var translation = await Translator.TranslateAsync(testMessage, "en", new() { language });
        var speech = await Speaker.SpeakAsync(translation.First());
        return new(speech?.Url);
    }
}