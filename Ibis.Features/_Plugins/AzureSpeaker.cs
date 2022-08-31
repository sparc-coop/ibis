using Ibis.Features.Sparc.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CognitiveServices.Speech;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features._Plugins;

public class AzureSpeaker : ISpeaker
{
    readonly HttpClient Client;
    readonly string SubscriptionKey;
    readonly IHubContext<RoomHub> Hub;

    public IRepository<File> Files { get; }

    public AzureSpeaker(IConfiguration configuration, IHubContext<RoomHub> hub, IRepository<File> files)
    {
        SubscriptionKey = configuration.GetConnectionString("Speech");

        Client = new HttpClient
        {
            BaseAddress = new Uri("	https://eastus.tts.speech.microsoft.com")
        };
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

        Hub = hub;
        Files = files;
    }

    public async Task<AudioMessage?> SpeakAsync(Message message)
    {
        if (message.Audio?.Voice == null)
            return null;

        var synthesizer = Synthesizer(message.Audio.Voice.ShortName);
        var words = new List<Word>();
        synthesizer.WordBoundary += (sender, e) =>
        {
            Hub.Clients.Group(message.Audio.Voice.Locale).SendAsync("WordBoundary", message.Id, e.AudioOffset, e.WordLength);
            words.Add(new((long)e.AudioOffset, e.Duration.Ticks, e.Text));
        };

        synthesizer.Synthesizing += (sender, e) =>
        {
            Hub.Clients.Group(message.Audio.Voice.Locale).SendAsync("Speak", message.Id, e.Result.AudioData);
        };

        var result = await synthesizer.SpeakTextAsync(message.Text);

        using var stream = new MemoryStream(result.AudioData, false);
        File file = new("speak", $"{message.RoomId}/{message.Id}/{message.Audio.Voice.ShortName}.wav", AccessTypes.Public, stream);
        await Files.AddAsync(file);

        return message.Audio with
        {
            Url = file.Url!,
            Duration = result.AudioDuration.Ticks,
            Subtitles = words
        };
    }

    public async Task<List<Voice>> GetVoicesAsync(string? language = null, string? dialect = null, string? gender = null)
    {
        var result = await Client.GetFromJsonAsync<List<Voice>>("/cognitiveservices/voices/list");

        return result!
            .Where(x => language == null || x.Locale.StartsWith(language))
            .Where(x => dialect == null || x.Locale.Split("-").Last() == dialect)
            .Where(x => gender == null || x.Gender == gender)
            .ToList();
    }

    SpeechSynthesizer Synthesizer(string voice)
    {
        var config = SpeechConfig.FromSubscription(SubscriptionKey, "eastus");
        config.SpeechSynthesisVoiceName = voice;

        return new SpeechSynthesizer(config, null);
    }
}
