using Microsoft.AspNetCore.Http.HttpResults;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Policy;
using File = Sparc.Blossom.Data.File;

namespace Ibis.Messages;

public record UploadImageRequest(string RoomId, string Language, byte[] Bytes, string FileName);

public class UploadImage : Feature<UploadImageRequest, string>
{
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public IFileRepository<File> Files { get; }
    public AzureOCR AzureOCR { get; }

    public UploadImage(IRepository<User> users, IRepository<Message> messages, IRepository<Room> rooms, IFileRepository<File> files, AzureOCR azureOCR)
    {
        Users = users;
        Messages = messages;
        Rooms = rooms;
        Files = files;
        AzureOCR = azureOCR;
    }

    public override async Task<string> ExecuteAsync(UploadImageRequest request)
    {
        var user = await Users.GetAsync(User);

        var imageUrl = await UploadImageToStorage(request.RoomId, request.FileName, request.Bytes);
        var OCRTextResult = await AzureOCR.MakeRequest(imageUrl);

        var message = new Message(request.RoomId, user.Avatar, OCRTextResult)
        {
            Type = "image",
            ImageUrl = imageUrl
        };

        await Messages.AddAsync(message);

        return "abc";
    }

    internal async Task<string> UploadImageToStorage(string roomId, string fileName, byte[] bytes)
    {
        File file = new("image", $"{roomId}/{fileName}.png", AccessTypes.Public, new MemoryStream(bytes));
        await Files.AddAsync(file);
        return file.Url!;
    }

}
