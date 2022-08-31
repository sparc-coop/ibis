using Newtonsoft.Json;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Users;

public record UploadUserPhotoRequest(string UserId, string FileName, byte[] Bytes);
public class UserPhotoUpload : Feature<UploadUserPhotoRequest, string>
{ 
	public UserPhotoUpload(IRepository<User> users, IRepository<File> files)
	{
		Users = users;
		Files = files;
	}
    public IRepository<User> Users { get; }
	public IRepository<File> Files { get; }

	public override async Task<string> ExecuteAsync(UploadUserPhotoRequest request)
    {
		try
		{
			string fileUrl = await UploadPhotoToStorage(request.UserId, request.FileName, request.Bytes);
			var user = await Users.FindAsync(User.Id());
			user!.ProfileImg = fileUrl;
			await Users.UpdateAsync(user);

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
