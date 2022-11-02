using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sparc.Authentication;
using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users;

public record InviteUserRequest(string Email, string RoomId);
public class InviteUser : PublicFeature<InviteUserRequest, string>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public UserManager<User> UserManager { get; }
    public IConfiguration Configuration { get; }

    public InviteUser(TwilioService twilio, IRepository<Room> rooms, IRepository<User> users, UserManager<User> userManager, IConfiguration configuration)
    {
        Twilio = twilio;
        Rooms = rooms;
        Users = users;
        UserManager = userManager;
        Configuration = configuration;
    }
    TwilioService Twilio { get; set; }


    public override async Task<string> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            string messageBody = "";

            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).First();
            var user = Users.Query.Where(u => u.Email == request.Email).FirstOrDefault();

            if (user != null) //check new or existing
            {
                messageBody = "You have been added to new room on Ibis! Click the link to join.";
            } else
            {
                user = new(request.Email, request.Email);
                await UserManager.CreateAsync(user);
                messageBody = "You have been invited to join Ibis! Sign up here.";
            }

            string roomLink = await UserManager.CreateMagicSignInLinkAsync(user, $"{Configuration["WebClientUrl"]}/rooms/{request.RoomId}");
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
}
