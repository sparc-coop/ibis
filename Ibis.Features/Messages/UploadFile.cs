using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Messages;

public record UploadFileRequest(string RoomId, string Language, byte[] Bytes, string FileName);

public class UploadFile : PublicFeature<UploadFileRequest, List<Message>>
{
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<File> Files { get; }

    public UploadFile(IRepository<User> users, IRepository<Message> messages, IRepository<Room> rooms, IRepository<File> files)
    {
        Users = users;
        Messages = messages;
        Rooms = rooms;
        Files = files;
    }

    public override Task<List<Message>> ExecuteAsync(UploadFileRequest request)
    {
        throw new NotImplementedException();
    }

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

}
