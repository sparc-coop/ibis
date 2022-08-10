using NAudio.Wave;
using Newtonsoft.Json;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Rooms;

public record MergeAudioRequest(IEnumerable<string> Files, string RoomId, string Language);
public class MergeAudio : PublicFeature<MergeAudioRequest, string>
{
    public MergeAudio(IRepository<Room> rooms, IRepository<File> files)
    {
        Rooms = rooms;
        Files = files;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<File> Files { get; }

    public async override Task<string> ExecuteAsync(MergeAudioRequest request)
    {
        string outputFile = "audio_output.wav";
        await Combine(outputFile, request);
        string roomUrl = await UpdateRoom(request.RoomId, "en");// request.Language);
        return JsonConvert.SerializeObject(roomUrl);
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
            FileInfo? fileInfo = new("audio.wav");
            var fileStream = fileInfo.OpenWrite();
            await stream.CopyToAsync(fileStream);
            fileStream.Close();

            using WaveFileReader reader = new WaveFileReader(fileStream.Name);
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

        waveFileWriter?.Dispose();

        return outputFile;
    }

    async Task<string> UpdateRoom(string roomId, string language)
    {
        var room = await Rooms.FindAsync(roomId);
        string url = "";

        using (var fileStream = new FileStream("audio_output.wav", FileMode.Open))
        {
            File file = new("speak", $"{roomId}/room/{language}.wav", AccessTypes.Public, fileStream);
            await Files.AddAsync(file);
            room!.SetAudio(file.Url!);
            await Rooms.UpdateAsync(room);
            url = file.Url!;
        }

        return url;
    }
}
