using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users;

public record InviteUserRequest(string RoomId, string Email);
public class InviteUser : Feature<InviteUserRequest, bool>
{

    public InviteUser(TwilioService twilio)
    {
        Twilio = twilio;
    }
    TwilioService Twilio { get; set; }

    public override async Task<bool> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            string subject = "Ibis Room Invitation";
            string message = "You have been invited to join a room with Ibis!";
            await Twilio.SendEmailAsync(request.Email, subject, message, "support@kuviocreative.com");

            //save user to room

            return true;
        } catch (Exception)
        {
            return false;
        }

    }
}
