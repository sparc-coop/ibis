using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users;

public record InviteUserRequest(string Email, string RoomId, string UserId);
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
            string message = "";
            string roomLink = "";

            //save user to room
            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).FirstOrDefault();
            var user = Users.Query.Where(u => u.UserId == request.UserId).FirstOrDefault();

            if (user.FullName != null) //check new or existing
            {
                roomLink = request.RoomId;
                message = "You have been added to new room on Ibis!";
            } else
            {
                message = "You have been invited to join Ibis!";
            }

            await Twilio.SendEmailAsync(request.Email, subject, message, "support@kuviocreative.com");

            //if (room != null & user != null)
            //{

            //    room.AddUser(request.UserId, "English");
            //    user.ActiveRooms.Add(new ActiveRoom(room.RoomId, "", DateTime.Now));
            //}

            var userList = new List<RoomUser>();
            //foreach(var item in room.ActiveUsers)
            //{
            //    User user = await Users.FindAsync(item.UserId);

            //}

                RoomUser roomUser = new RoomUser(Name: "Invitee", Initials: "TBD", Email: request.Email);
            //roomUser.Email = invitee.Email;
            //roomUser.Name = invitee.DisplayName;
            //roomUser.Initials = invitee.Initials;
            //userList.Add(roomUser);

            //GetRoomResponse response = await Api.CreateRoomAsync(new NewRoomRequest { RoomName = Name, Participants = userList });


            return true;
        } catch (Exception)
        {
            return false;
            //return new RoomUser(Name: "Invitee", Initials: "TBD", Email: "None");
        }

    }
}
