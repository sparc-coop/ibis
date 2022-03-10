using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Users
{
    public class UpdateUser : Feature<string, GetUserResponse>
    {
        public override Task<GetUserResponse> ExecuteAsync(string request)
        {
            throw new NotImplementedException();
        }
    }
}
