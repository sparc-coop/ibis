using Ibis.Features.Conversations.Entities;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using Sparc.Core;
using Sparc.Storage.Azure;
using System.Text;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features._Plugins
{
    public class IbisEngine
    {
        HttpClient Translator { get; set; }
        string SpeechApiKey { get; set; }
        public IRepository<File> Files { get; }
        public IRepository<Message> Message { get; }
        public IRepository<Conversation> Conversations { get; }

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
            var result = await synthesizer.SpeakTextAsync(message.Text);
            using var stream = new MemoryStream(result.AudioData, false);

            File file = new("speak", $"{message.ConversationId}/{message.Id}/{message.Language}.wav", AccessTypes.Public, stream);
            await Files.AddAsync(file);
            
            message.SetAudio(file.Url!);

            await Parallel.ForEachAsync(message.Translations, async (translation, token) =>
            {
                await SpeakAsync(message, translation);
            });
        }

        internal async Task SpeakAsync(Message parentMessage, Conversations.Entities.Translation message)
        {
            var config = SpeechConfig.FromSubscription(SpeechApiKey, "eastus");
            config.SpeechSynthesisLanguage = message.Language;

            using var synthesizer = new SpeechSynthesizer(config, null);
            var result = await synthesizer.SpeakTextAsync(message.Text);
            using var stream = new MemoryStream(result.AudioData, false);

            Sparc.Storage.Azure.File file = new("speak", $"{parentMessage.ConversationId}/{parentMessage.Id}/{message.Language}.wav", AccessTypes.Public, stream);
            await Files.AddAsync(file);

            message.SetAudio(file.Url!);
        }

        internal async Task TranslateAsync(Message message, List<Language> languages)
        {
            var otherLanguages = languages.Where(x => x.Name != message.Language).Select(x => x.Name).ToArray();
            if (otherLanguages.Any())   
                await TranslateAsync(message, otherLanguages);
        }

        internal async Task TranslateAsync(Message message, params string[] languages)
        {
            object[] body = new object[] { new { message.Text } };
            var from = $"&from={message.Language.Split('-').First()}";
            var to = "&to=" + string.Join("&to=", languages.Select(x => x.Split('-').First()));

            var result = await Post<TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);

            foreach (TranslationResult o in result)
                foreach (Translation t in o.Translations)
                    message.AddTranslation(languages.First(x => x.StartsWith(t.To, StringComparison.InvariantCultureIgnoreCase)), t.Text);
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

        //internal async Task<string> UploadFile(Conversation conversation, string language, FileStream fileStream)
        //{
        //    string url = "";

        //    Sparc.Storage.Azure.File file = new("speak", $"{conversation.Id}/conversation/{language}.wav", AccessTypes.Public, fileStream);
        //    await Files.AddAsync(file);
        //    conversation.SetAudio(file.Url!);
        //    await Conversations.UpdateAsync(conversation);
        //    url = file.Url!;

        //    return url;
        //}

        //internal async Task<Message> TranscribeSpechFromUpload()

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
}
