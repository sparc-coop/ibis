﻿namespace Ibis.Users;

public record ChangeVoiceRequest(string Language, string? VoiceName);
public record ChangeVoiceResponse(string? PreviewAudioUrl);
public class ChangeVoice : Feature<ChangeVoiceRequest, ChangeVoiceResponse>
{
    public ChangeVoice(IRepository<User> users, IRepository<Room> rooms, Translator translator, ISpeaker speaker)
    {
        Users = users;
        Rooms = rooms;
        Translator = translator;
        Speaker = speaker;
    }

    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public Translator Translator { get; }
    public ISpeaker Speaker { get; }

    public override async Task<ChangeVoiceResponse> ExecuteAsync(ChangeVoiceRequest request)
    {
        var user = await Users.GetAsync(User);

        var language = await Translator.GetLanguageAsync(request.Language) 
            ?? throw new Exception("Language not found!");
        
        if (request.VoiceName == null) // default voice
        {
            user!.ChangeVoice(language);
            await Users.UpdateAsync(user!);
            return new(null);
        }
        else
        {
            var voices = await Speaker.GetVoicesAsync(request.Language);
            var voice = voices.FirstOrDefault(x => x.ShortName == request.VoiceName)
                ?? throw new Exception("Voice doesn't match language!");

            user!.ChangeVoice(language, voice);
            await Users.UpdateAsync(user!);

            var speech = await GenerateVoiceSample(user, language);
            return new(speech?.Url);
        }
    }

    private async Task<AudioMessage?> GenerateVoiceSample(User user, Language language)
    {
        var testMessages = new List<Message>
        {
            new Message("", user, $"Hi, nice to meet you!", "en"),
            new Message("", user, $"Hey, how are things going today?", "en"),
            new Message("", user, $"What time do you want to meet?", "en"),
            new Message("", user, $"Thanks, talk to you later!", "en")
        };

        int index = new Random().Next(testMessages.Count);
        var testMessage = testMessages[index];

        var translation = await Translator.TranslateAsync(testMessage, new() { language });
        var speech = await Speaker.SpeakAsync(translation.First());
        return speech;
    }
}