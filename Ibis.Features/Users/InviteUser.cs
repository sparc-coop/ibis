using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users;

public record InviteUserRequest(string Email, string RoomId);
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
            string subject = "Ibis Invitation";
            string messageBody = "";
            string roomLink = "";

            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).First();
            var user = Users.Query.Where(u => u.Email == request.Email).FirstOrDefault();

            if (user != null) //check new or existing
            {
                roomLink = request.RoomId;
                messageBody = "You have been added to new room on Ibis! Click the link to join.";
            } else
            {
                messageBody = "You have been invited to join Ibis! Sign up here.";
            }

            await Twilio.SendEmailAsync(request.Email, subject, messageBody, "margaret@kuviocreative.com");

            //save user to room
            if (user != null)
            {
                room.AddUser(user.UserId, user.PrimaryLanguageId, user.ProfileImg);
                user.ActiveRooms.Add(new ActiveRoom(room.RoomId, "", DateTime.Now));
            }

            //add pending user if not yet signed up
            if (user == null && !room.PendingUsers.Contains(request.Email))
            {
                room.PendingUsers.Add(request.Email);
            }

            await Rooms.UpdateAsync(room);

            return true;

        } catch (Exception ex)
        {
            var test = ex.Message;
            return false;
        }

    }
}
