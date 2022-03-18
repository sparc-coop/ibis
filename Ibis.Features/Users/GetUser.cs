﻿using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Users
{
    public record GetUserResponse(string FullName, string Emial);
    public class GetUser : Feature<string, GetUserResponse>
    {
        public IRepository<User> Users { get; }
        public GetUser(IRepository<User> users)
        {
            Users = users;
        }
        public override async Task<GetUserResponse> ExecuteAsync(string userId)
        {
            User user = await Users.FindAsync(userId);
            return new(user.FirstName, user.Email);
        }
    }
}