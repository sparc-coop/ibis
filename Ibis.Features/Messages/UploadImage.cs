using File = Sparc.Blossom.Data.File;

namespace Ibis.Messages;

public record UploadFileRequest(string RoomSlug, string Language, string Tag);

public class UploadFile(IRepository<Room> rooms, IFileRepository<File> files, TypeMessage typeMessage)
{
    public IRepository<Room> Rooms { get; } = rooms;
    public IFileRepository<File> Files { get; } = files;
    public TypeMessage TypeMessage { get; } = typeMessage;

    public async Task<Message> ExecuteAsync(UploadFileRequest request, MemoryStream stream)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug) 
            ?? throw new Exception("Room not found.");
        
        var url = await UploadImageToStorage(room.RoomId, stream);
        var newMessage = new TypeMessageRequest(request.RoomSlug, request.Language, url, request.Tag);
        return await TypeMessage.ExecuteAsUserAsync(newMessage, User.System);

    }

    internal async Task<string> UploadImageToStorage(string roomId, MemoryStream stream)
    {
        var randomFileName = Guid.NewGuid().ToString();
        File file = new("images", $"{roomId}/upload/{randomFileName}.png", AccessTypes.Public, stream);
        await Files.AddAsync(file);
        return file.Url!;
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
