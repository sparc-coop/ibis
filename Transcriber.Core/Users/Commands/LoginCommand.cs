using Kuvio.Kernel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transcriber.Core;

namespace Transcriber.Core.Users.Commands
{
    public class LoginCommand
    {
        private readonly IRepository<User> _usersRepository;
       

        public LoginCommand(IRepository<User> users)
        {
            _usersRepository = users;
            
        }

        public User Execute(ClaimsPrincipal principal, string azureId, string email, string displayName)
        {
           
            var user = _usersRepository.FindAsync(x => x.UserIdentity.Any(y => y.AzureID == azureId)).Result;

            if (user == null)
            {
                user = _usersRepository.FindAsync(x => email.ToLower() == x.Email).Result;

                if (user == null)
                {
                    user = new User(displayName, email, azureId);
                    _usersRepository.AddAsync(user).Wait();
                }
            }

            _usersRepository.ExecuteAsync(user, u => u.Login(principal, azureId)).Wait();

            return user;
        }

        public async Task<User> ExecuteAsync(ClaimsPrincipal principal, string azureId, string email, string displayName)
        {
         
            var user = await _usersRepository.FindAsync(x => x.UserIdentity.Any(y => y.AzureID == azureId));

            if (user == null)
            {
                user = await _usersRepository.FindAsync(x => !string.IsNullOrEmpty(email) && email.ToLower() == x.Email); // User without identity

                if (user == null)
                {
                    user = new User(displayName, email, azureId);
                    await _usersRepository.AddAsync(user);
                }
            }

            await _usersRepository.ExecuteAsync(user, u => u.Login(principal, azureId));

            return user;
        }
    }
}
