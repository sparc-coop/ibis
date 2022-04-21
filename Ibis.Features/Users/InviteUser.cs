using Ibis.Features._Plugins;
using Sparc.Core;
using Sparc.Features;
using Sparc.Notifications.Twilio;

namespace Ibis.Features.Users
{
    public record InviteUserRequest(string conversationId, string email);
    public class InviteUser : Feature<InviteUserRequest, bool>
    {

        public InviteUser(IRepository<User> users, TwilioService twilio)
        {
            Users = users;
            Twilio = twilio;
        }
        IRepository<User> Users { get; set; }
        TwilioService Twilio { get; set; }
        public override async Task<bool> ExecuteAsync(InviteUserRequest request)
        {
            try
            {
                string subject = "Ibis Conversation Invitation";
                string message = "You have been invited to join conversation with Ibis!";
                await Twilio.SendEmailAsync(request.email, subject, message, "support@kuv.io");

                //save user to conversation

                return true;
            } catch (Exception ex)
            {
                return false;
            }

        }
    }
}
