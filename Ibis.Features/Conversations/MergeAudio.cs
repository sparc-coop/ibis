using NAudio.Wave;
using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;
using System.Net;

namespace Ibis.Features.Conversations
{
    public class MergeAudio : PublicFeature<string>
    {

        public static async Task Combine(string outputFile, IEnumerable<string> inputFiles)//, Stream output)
        {
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.);    
            //"C:\\Users\\Margaret Landefeld\\Source\\Sparc\\ibis\\Ibis.Features\\mergedFile.wav";

            byte[] buffer = new byte[1024];
            WaveFileWriter? waveFileWriter = null;

            foreach (string sourceFile in inputFiles)
            {
                //var newsourceFile = new FileStream("https://ibistranscriber.blob.core.windows.net/speak/3a503f13-2a84-445b-b6ba-e2b06ebdf86c/9433c4c9-5cd4-450f-a98b-d18c763d9fd4/en-US.wav", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
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
                        waveFileWriter.WriteData(buffer, 0, read);
                    }
                }
            }

            waveFileWriter.Dispose();
        }

        public MergeAudio()
        {

        }

        public override async Task<string> ExecuteAsync()
        {
            string outputFile = "testing123.wav";
                ////"C:\\Users\\Margaret Landefeld\\Source\\Sparc\\ibis\\Ibis.Features\\en-US.wav";// 
            string file1 = "https://ibistranscriber.blob.core.windows.net/speak/3a503f13-2a84-445b-b6ba-e2b06ebdf86c/9433c4c9-5cd4-450f-a98b-d18c763d9fd4/en-US.wav";
            string file2 = "https://ibistranscriber.blob.core.windows.net/speak/3a503f13-2a84-445b-b6ba-e2b06ebdf86c/c0c549e5-d66b-4429-9372-7830256d7e4a/en-US.wav";
            await Combine(outputFile, new string[] { file1, file2 });
            return "test";
        }
    }
}
