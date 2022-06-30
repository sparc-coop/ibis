using Newtonsoft.Json;

namespace Ibis.Features.Users;

public record UploadUserPhotoRequest(string UserId, string FileName, byte[] Bytes);
public class UserPhotoUpload : Feature<UploadUserPhotoRequest, string>
{ 
	public UserPhotoUpload(IRepository<User> users, IbisEngine ibisEngine)
	{
		Users = users;
		IbisEngine = ibisEngine;
	}
	public IbisEngine IbisEngine { get; }
    public IRepository<User> Users { get; }
    public override async Task<string> ExecuteAsync(UploadUserPhotoRequest request)
    {
        try
		{
			string fileUrl = await IbisEngine.UploadPhotoToStorage(request.UserId, request.FileName, request.Bytes);
			var user = await Users.FindAsync(User.Id());
			user.ProfileImg = fileUrl;
			await Users.UpdateAsync(user);

			return JsonConvert.SerializeObject(fileUrl);

		} catch(Exception ex)
		{
            return "";
		}
    }
}
