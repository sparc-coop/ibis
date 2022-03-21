using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Users
{
    public class UpdateUser : Feature<string, bool>
    {
        public IRepository<User> Users { get; }
        public UpdateUser(IRepository<User> users)
        {
            Users = users;
        }
        public override async Task<bool> ExecuteAsync(string request)
        {
            try
            {

                return true;
            } 
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
