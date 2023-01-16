using Newtonsoft.Json;
using File = Sparc.Blossom.Data.File;

namespace Ibis.Users;

public record UploadUserPhotoRequest(string FileName, byte[] Bytes);
public class UserPhotoUpload : Feature<UploadUserPhotoRequest, string>
{ 
	public UserPhotoUpload(IRepository<User> users, IFileRepository<File> files)
	{
		Users = users;
		Files = files;
	}
    public IRepository<User> Users { get; }
	public IFileRepository<File> Files { get; }

	public override async Task<string> ExecuteAsync(UploadUserPhotoRequest request)
    {
		try
		{
			string fileUrl = await UploadPhotoToStorage(User.Id(), request.FileName, request.Bytes);
			var user = await Users.GetAsync(User);
			//user!.ProfileImg = fileUrl;
			await Users.UpdateAsync(user!);

			return JsonConvert.SerializeObject(fileUrl);

		}
		catch (Exception)
		{
            return "";
		}
    }

    internal async Task<string> UploadPhotoToStorage(string userId, string filename, byte[] bytes)
    {
        File file = new("account-photos", $"{userId}/{filename}", AccessTypes.Public, new MemoryStream(bytes));
        await Files.AddAsync(file);
        return file.Url!;
    }
}
