using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users;

public record InviteUserRequest(string Email, string RoomId);
public class InviteUser : PublicFeature<InviteUserRequest, string>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public UserManager<User> UserManager { get; }

    public InviteUser(TwilioService twilio, IRepository<Room> rooms, IRepository<User> users, UserManager<User> userManager)
    {
        Twilio = twilio;
        Rooms = rooms;
        Users = users;
        UserManager = userManager;
    }
    TwilioService Twilio { get; set; }


    public override async Task<string> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            string messageBody = "";
            string roomLink = await CreateMagicSignInLinkAsync(request.Email, $"https://ibis.chat/rooms/{request.RoomId}");

            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).First();
            var user = Users.Query.Where(u => u.Email == request.Email).FirstOrDefault();

            if (user != null) //check new or existing
            {
                messageBody = "You have been added to new room on Ibis! Click the link to join.";
            } else
            {
                user = new(request.Email, request.Email);
                messageBody = "You have been invited to join Ibis! Sign up here.";
            }

            //await Twilio.SendEmailAsync(request.Email, subject, messageBody, "margaret@kuviocreative.com");

            //add pending user
            //await Rooms.ExecuteAsync(request.RoomId, r => r.InviteUser(user));

            //await Rooms.UpdateAsync(room);

            return roomLink;

        } catch (Exception ex)
        {
            var test = ex.Message;
            return test;
        }

    }

    async Task<string> CreateMagicSignInLinkAsync(string email, string roomUrl)
    {
        var user = Users.Query.FirstOrDefault(x => x.Email == email);
        if (user == null)
        {
            user = new(Guid.NewGuid().ToString(), email);
            await Users.UpdateAsync(user);
        }

        if (user.SecurityStamp == null)
            await UserManager.UpdateSecurityStampAsync(user);

        var token = await UserManager.GenerateUserTokenAsync(user, "Default", "passwordless-auth");
        return token ?? "";
    }
}
