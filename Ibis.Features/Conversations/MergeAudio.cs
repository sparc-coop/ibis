using NAudio.Wave;
using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;
using Newtonsoft.Json;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Conversations
{
    public record MergeAudioRequest(IEnumerable<string> Files, string ConversationId, string Language);
    public class MergeAudio : PublicFeature<MergeAudioRequest, string>
    {
        public MergeAudio(IbisEngine ibisEngine, IRepository<Conversation> conversations, IRepository<File> files)
        {
            IbisEngine = ibisEngine;
            Conversations = conversations;
            Files = files;
        }
        public IbisEngine IbisEngine { get; }
        public IRepository<Conversation> Conversations { get; }
        public IRepository<File> Files { get; }

        public async override Task<string> ExecuteAsync(MergeAudioRequest request)
        {
            string outputFile = "audio_output.wav";
            string result = await Combine(outputFile, request);
            string conversationUrl = await UpdateConversation(request.ConversationId, "en-US");// request.Language);
            return JsonConvert.SerializeObject(conversationUrl);
        }

        public static async Task<string> Combine(string outputFile, MergeAudioRequest request)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter? waveFileWriter = null;

            foreach (string sourceFile in request.Files)
            {
                HttpClient client = new();
                var response = await client.GetAsync(sourceFile);

                Stream stream = await response.Content.ReadAsStreamAsync();
                FileInfo? fileInfo = new FileInfo("audio.wav");
                var fileStream = fileInfo.OpenWrite();
                await stream.CopyToAsync(fileStream);
                fileStream.Close();

                using (WaveFileReader reader = new WaveFileReader(fileStream.Name))
                {
                    if (waveFileWriter == null)
                    {
                        // first time in create new Writer
                        waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                    }
                    else
                    {
                        if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                        {
                            throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                        }
                    }

                    int read;
                    while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        waveFileWriter.Write(buffer, 0, read);
                    }
                }
            }

            waveFileWriter.Dispose();

            return outputFile;
        }

        async Task<string> UpdateConversation(string conversationId, string language)
        {
            Conversation conversation = await Conversations.FindAsync(conversationId);
            string url = "";

            using (var fileStream = new FileStream("audio_output.wav", FileMode.Open))
            {
                Sparc.Storage.Azure.File file = new("speak", $"{conversationId}/conversation/{language}.wav", AccessTypes.Public, fileStream);
                await Files.AddAsync(file);
                conversation.SetAudio(file.Url!);
                await Conversations.UpdateAsync(conversation);
                url = file.Url!;
            }

            return url;
        }
    }
}
