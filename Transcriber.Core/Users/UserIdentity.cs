using System;
using System.Collections.Generic;
using System.Text;

namespace Transcriber.Core.Users
{
    public class UserIdentity
    {
        private UserIdentity()
        {

        }

        public UserIdentity(string externalUserId, string userId, string identityProvider)
        {
            AzureID = externalUserId;
            IdentityProvider = identityProvider;
            CreateDate = DateTime.UtcNow;
            UserId = userId;
            LastLoginDate = CreateDate;
        }

        public string AzureID { get; set; }
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }

        public User User { get; set; }
    }
}
