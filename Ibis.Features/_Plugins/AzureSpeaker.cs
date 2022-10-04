using Ibis.Features.Sparc.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CognitiveServices.Speech;
using NAudio.Lame;
using NAudio.Wave;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features._Plugins;

public record WordSpoken(string UserId, string Language, byte[] Audio, List<Word> Words) : SparcNotification(UserId + "|" + Language);
public class AzureSpeaker : ISpeaker
{
    readonly HttpClient Client;
    readonly string SubscriptionKey;
    readonly IHubContext<IbisHub> Hub;

    public IRepository<File> Files { get; }

    public AzureSpeaker(IConfiguration configuration, IHubContext<IbisHub> hub, IRepository<File> files)
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

        var synthesizer = Synthesizer(message.Audio.Voice);
        var words = new List<Word>();
        synthesizer.WordBoundary += (sender, e) =>
        {
            words.Add(new((long)e.AudioOffset / 10000, (long)e.Duration.TotalMilliseconds, e.Text));
        };

        synthesizer.SynthesisCompleted += (sender, e) =>
        {
            var word = new WordSpoken(message.User.Id, message.Language, ConvertWavToMp3(e.Result.AudioData), words);
            Hub.Clients.Group(message.User.Id + "|" + message.Language)
                .SendAsync(typeof(WordSpoken).Name, word);
        };

        var result = await synthesizer.SpeakTextAsync(message.Text);

        using var stream = new MemoryStream(ConvertWavToMp3(result.AudioData), false);
        File file = new("speak", $"{message.RoomId}/{message.Id}/{message.Audio.Voice}.mp3", AccessTypes.Public, stream);
        await Files.AddAsync(file);

        var cost = message.Text!.Length / 1_000_000M * -16.00M; // $16 per 1M characters
        message.AddCharge(cost, $"Speak message from {message.User.Name} in voice {message.Audio!.Voice}");
        
        return new(file.Url!, (long)result.AudioDuration.TotalMilliseconds, message.Audio.Voice, words);
    }

    public async Task<AudioMessage> SpeakAsync(List<Message> messages)
    {
        byte[] buffer = new byte[1024];
        WaveFileWriter? waveFileWriter = null;
        using var combinedAudio = new MemoryStream();

        foreach (var message in messages.Where(x => x.Audio?.Url != null).OrderBy(x => x.Timestamp))
        {
            HttpClient client = new();
            var inputAudio = await client.GetStreamAsync(message.Audio!.Url);

            using WaveFileReader reader = new(inputAudio);
            if (waveFileWriter == null)
            {
                // first time in create new Writer
                waveFileWriter = new WaveFileWriter(combinedAudio, reader.WaveFormat);
            }
            else if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
            {
                throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
            }

            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                waveFileWriter.Write(buffer, 0, read);
            }
        }

        waveFileWriter?.Dispose();

        File file = new("speak", $"{messages.First().RoomId}/{messages.First().Audio!.Voice}.wav", AccessTypes.Public, combinedAudio);
        await Files.AddAsync(file);

        return new(file.Url, 0, messages.First().Audio!.Voice);
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

    public static byte[] ConvertWavToMp3(byte[] wavFile)
    {
        using var retMs = new MemoryStream();
        using var ms = new MemoryStream(wavFile);
        using var rdr = new WaveFileReader(ms);
        using var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 128);
        rdr.CopyTo(wtr);
        wtr.Flush();
        return retMs.ToArray();
    }
}
