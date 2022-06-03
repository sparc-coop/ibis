using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using Sparc.Storage.Azure;
using System.Text;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features._Plugins;

public class IbisEngine
{
    HttpClient Translator { get; set; }
    string SpeechApiKey { get; set; }
    public IRepository<File> Files { get; }

    public IbisEngine(IConfiguration configuration, IRepository<File> files)
    {
        Translator = new HttpClient
        {
            BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com"),
        };
        Translator.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetConnectionString("Translator"));
        Translator.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "southcentralus");

        SpeechApiKey = configuration.GetConnectionString("Speech");
        Files = files;
    }

    internal async Task SpeakAsync(Message message)
    {
        var config = SpeechConfig.FromSubscription(SpeechApiKey, "eastus");
        config.SpeechSynthesisLanguage = message.Language;

        using var synthesizer = new SpeechSynthesizer(config, null);
        var text = message.ModifiedText ?? message.Text;
        var result = await synthesizer.SpeakTextAsync(text);
        using var stream = new MemoryStream(result.AudioData, false);

        Sparc.Storage.Azure.File file;
        if (message.ModifiedText == null)
        {
            file = new("speak", $"{message.RoomId}/{message.Id}/{message.Language}.wav", AccessTypes.Public, stream);
            await Files.AddAsync(file);
            message.SetAudio(file.Url!);
        }
        else
        {
            file = new("speak", $"{message.RoomId}/{message.Id}/{message.Language}__modified.wav", AccessTypes.Public, stream);
            await Files.AddAsync(file);
            message.SetModifiedAudio(file.Url!);
        }

        await Parallel.ForEachAsync(message.Translations, async (translation, token) =>
        {
            await SpeakAsync(message, translation);
        });
    }

    internal async Task SpeakAsync(Message parentMessage, Messages.Translation message)
    {
        var config = SpeechConfig.FromSubscription(SpeechApiKey, "eastus");
        config.SpeechSynthesisLanguage = message.Language;

        using var synthesizer = new SpeechSynthesizer(config, null);
        var text = message.ModifiedText ?? message.Text;
        var result = await synthesizer.SpeakTextAsync(text);
        using var stream = new MemoryStream(result.AudioData, false);

        Sparc.Storage.Azure.File file = new("speak", $"{parentMessage.RoomId}/{parentMessage.Id}/{message.Language}.wav", AccessTypes.Public, stream);
        await Files.AddAsync(file);

        message.SetAudio(file.Url!);
    }

    internal async Task TranslateAsync(Message message, List<Language> languages)
    {
        var otherLanguages = languages.Where(x => x.Name != message.Language).Select(x => x.Name).ToArray();
        if (otherLanguages.Any())
            await TranslateAsync(message, otherLanguages);
    }

    internal async Task<Message> TranslateAsync(Message message, params string[] languages)
    {
        var text = message.ModifiedText ?? message.Text;
        object[] body = new object[] { new { text } };
        var from = $"&from={message.Language.Split('-').First()}";
        var to = "&to=" + string.Join("&to=", languages.Select(x => x.Split('-').First()));

        var result = await Post<TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);

        foreach (TranslationResult o in result)
            foreach (Translation t in o.Translations)
                message.AddTranslation(languages.First(x => x.StartsWith(t.To, StringComparison.InvariantCultureIgnoreCase)), t.Text);

        return message;
    }

    internal async Task<Message> TranscribeSpeechFromMic(Message message)
    {
        var speechConfig = SpeechConfig.FromSubscription(SpeechApiKey, "eastus");
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        try
        {
            using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
            {
                //Asks user for mic input and prints transcription result on screen
                Console.WriteLine("Speak into your microphone.");
                var result = await recognizer.RecognizeOnceAsync();
                Console.WriteLine($"RECOGNIZED: Text={result.Text}");

                message.SetText(result.Text);
                return message;
            }
        }
        catch (Exception ex)
        {
            var testing = ex.Message;
        }
        return message;
    }

    internal async Task<List<Message>> TranscribeSpeechFromFile(Message message, byte[] bytes, string fileName)
    {
        var speechConfig = SpeechConfig.FromSubscription(SpeechApiKey, "eastus");
        var audioConfig = IbisHelpers.OpenWavFile(bytes);

        var messages = new List<Message>();

        try
        {
            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
            var stopRecognition = new TaskCompletionSource<int>();

            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    Message newMessage = new(message.SubroomId!, message.UserId, message.Language, SourceTypes.Upload, message.UserName, message.UserInitials);
                    newMessage.SetTimestamp(e.Result.OffsetInTicks, e.Result.Duration);
                    newMessage.SetText(e.Result.Text);
                    if (message.SubroomId != null)
                        newMessage.SetSubroomId(message.SubroomId);
                    messages.Add(newMessage);
                }
            };

            recognizer.SessionStopped += (s, e) =>
            {
                stopRecognition.TrySetResult(0);
            };

            Console.WriteLine("Transcribing wav file...");
            await recognizer.StartContinuousRecognitionAsync();
            Task.WaitAny(new[] { stopRecognition.Task });
            return messages;
        }
        catch (Exception ex)
        {
            var testing = ex.Message;
            return new();
        }
    }

    //internal async Task<Message> UploadAudioToStorage(Message message, byte[] bytes)
    //{
    //    File file = new("speak", $"{message.RoomId}/{message.Id}/{message.Language}.wav", AccessTypes.Public, new MemoryStream(bytes));
    //    await Files.AddAsync(file);
    //    message.SetAudio(file.Url!);

    //    return message;
    //}
    internal async Task<string> UploadAudioToStorage(Room subroom, byte[] bytes)
    {
        File file = new("speak", $"{subroom.RoomId}/upload/original.wav", AccessTypes.Public, new MemoryStream(bytes));
        await Files.AddAsync(file);
        return file.Url!;
    }

    internal async Task<string> UploadVideoToStorage(string roomId, string fileName, byte[] bytes)
    {
        File file = new("speak", $"{roomId}/video/{fileName}.mp4", AccessTypes.Public, new MemoryStream(bytes));
        await Files.AddAsync(file);
        return file.Url!;
    }

    public async Task<List<KeyValuePair<string, LanguageItem>>> GetAllLanguages()
    {
        var response = await Translator.GetAsync("/languages?api-version=3.0&scope=translation");
        var result = await UnJsonify<LanguageTest>(response);
        return result.translation.ToList();
    }

    private async Task<T> Post<T>(string url, object model)
    {
        var response = await Translator.PostAsync(url, Jsonify(model));
        return await UnJsonify<T>(response);
    }

    private StringContent Jsonify(object model)
    {
        var json = JsonConvert.SerializeObject(model);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<T> UnJsonify<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<T>(result);
        }
        catch (Exception)
        {
            return default;
        }
    }
}

public record TranslationResult(DetectedLanguage DetectedLanguage, TextResult SourceText, Translation[] Translations);

public record DetectedLanguage(string Language, float Score);

public record TextResult(string Text, string Script); 

public record Translation(string Text, TextResult Transliteration, string To, Alignment Alignment, SentenceLength SentLen);

public record Alignment(string Proj);

public record SentenceLength(int[] SrcSentLen, int[] TransSentLen);

public record Test(LanguageTest group);
public record LanguageTest(Dictionary<string, LanguageItem> translation);//dictionary of languages //List<LanguageItem>> translation);//
public record LanguageItem(string name, string nativeName, string dir);
public record TranslationDict(List<LanguageItem> items);
