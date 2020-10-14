using Kuvio.Kernel.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Transcriber.Core.Users
{
    public class User : CosmosDbRootEntity
    {
        [JsonProperty("id")]
        public override string Id { get; set; }
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _email = null;
                    return;
                }

                _email = value.Trim().ToLower();
            }
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
        public string Initials => FirstName?.FirstOrDefault().ToString().ToUpper() + LastName?.FirstOrDefault().ToString().ToUpper();
        public string ProfilePictureUrl { get; set; }
       
        
        public ICollection<UserIdentity> UserIdentity { get; set; }
        
        public string SubscriptionId { get; set; }
        public DateTime? ActiveUntilDate { get; set; }
       

        public User(string firstName, string lastName, string email, string azureId) : base()
        {
            Id = Guid.NewGuid().ToString();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
          

            
            UserIdentity = new List<UserIdentity>();
            
            
            if (!String.IsNullOrWhiteSpace(azureId))
            {
                GetOrCreateIdentity(azureId);
            }
        }

       

    
        public List<Claim> GenerateClaims()
        {
            var claims = new List<Claim>
            {
                new Claim("ID", Id.ToString()),
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                //new Claim(ClaimTypes.Role, Role.Value),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", UserIdentity.First().AzureID),
                
            };

            if (!String.IsNullOrWhiteSpace(ProfilePictureUrl))
            {
                claims.Add(new Claim("ProfilePictureUrl", ProfilePictureUrl));
            }

            return claims;
        }

        public UserIdentity GetOrCreateIdentity(string externalUserId)
        {
            var identity = UserIdentity?.SingleOrDefault(x => x.AzureID == externalUserId);

            if (identity == null)
            {
                identity = new UserIdentity(externalUserId, Id, "Azure");
                UserIdentity.Add(identity);
                //AzureId = externalUserId;
            }
            return identity;
        }

        public void Login(ClaimsPrincipal principal, string externalUserId)
        {
            var identity = GetOrCreateIdentity(externalUserId);
            identity.LastLoginDate = DateTime.UtcNow;

            List<Claim> claims = GenerateClaims();

            // Logging in is simply adding claims to the existing principal
            foreach (var claim in claims.Where(x => !principal.HasClaim(y => y.Type == x.Type)))
            {
                (principal.Identity as ClaimsIdentity)?.AddClaim(claim);
            }
        }
    }
}
