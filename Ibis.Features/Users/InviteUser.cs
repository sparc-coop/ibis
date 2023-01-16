﻿using Microsoft.AspNetCore.Identity;

namespace Ibis.Users;

public record InviteUserRequest(string Email, string RoomId);
public class InviteUser : Feature<InviteUserRequest, UserAvatar?>
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


    public override async Task<UserAvatar?> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).First();
            var invitingUser = await Users.GetAsync(User);
            var user = Users.Query.Where(u => u.Email == request.Email).FirstOrDefault();

            if (user == null)
            {
                user = new(request.Email, request.Email);
                await UserManager.CreateAsync(user);
            }

            string roomLink = await UserManager.CreateMagicSignInLinkAsync(user, $"{Configuration["WebClientUrl"]}/rooms/{request.RoomId}");
            roomLink = $"{Request.Scheme}://{Request.Host.Value}{roomLink}";

            var templateData = new
            {
                RoomName = room.Name,
                InvitingUser = invitingUser?.Avatar.Name ?? "your friend",
                RoomLink =  roomLink
            };

            await Twilio.SendEmailTemplateAsync(request.Email, "d-f6bdbef00daf4780adb9ec3816193237", templateData);

            return user.Avatar;

        } catch (Exception ex)
        {
            var test = ex.Message;
            return null;
        }

    }
}
