using File = Sparc.Blossom.Data.File;

namespace Ibis.Messages;

public record GetFileFromStorageRequest(string RoomId, string? FileUrl);
public class GetFileFromStorage : Feature<GetFileFromStorageRequest, string>
{
    public GetFileFromStorage(IFileRepository<File> files, ISharedService sharedService)
    {
        Files = files;
        _sharedService = sharedService;
    }

    private readonly IFileRepository<File> Files;
    private readonly ISharedService _sharedService;


    public override async Task<string> ExecuteAsync(GetFileFromStorageRequest request)
    {
        if (_sharedService.FileUrl != null)
        {
            return _sharedService.FileUrl;
        }
        else
        {
            return "file_not_found";
        }

        //var file = await Files.FindAsync($"screenshots/{request.RoomId}/screenshot.png");

        //if (file != null)
        //{
        //    string fileUrl = file.Url;
        //    return fileUrl;
        //}
        //else
        //{
        //    return "file_not_found";
        //}
    }
}

