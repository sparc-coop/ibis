using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users;

public record InviteUserRequest(string RoomId, string Email, string UserId);
public class InviteUser : Feature<InviteUserRequest, bool>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public InviteUser(TwilioService twilio, IRepository<Room> rooms, IRepository<User> users)
    {
        Twilio = twilio;
        Rooms = rooms;
        Users = users;
    }
    TwilioService Twilio { get; set; }


    public override async Task<bool> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            string subject = "Ibis Room Invitation";
            string message = "You have been invited to join a room with Ibis!";
            await Twilio.SendEmailAsync(request.Email, subject, message, "support@kuviocreative.com");

            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).FirstOrDefault();
            var user = Users.Query.Where(u => u.UserId == request.UserId).FirstOrDefault();

            if (room != null & user != null)
            {
                room.AddUser(request.UserId, "English");
                user.ActiveRooms.Add(new ActiveRoom(room.RoomId, "", DateTime.Now));
            }

            //save user to room
            return true;
        } catch (Exception)
        {
            return false;
        }

    }
}
