using Ibis._Plugins;
using Microsoft.AspNetCore.Identity;
using Sparc.Ibis;

namespace Ibis.Users;

public record InviteUserRequest(string Email, string RoomId, string? Language);
public class InviteUser : Feature<InviteUserRequest, UserAvatar?>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public PasswordlessAuthenticator<User> Authenticator { get; }
    public IConfiguration Configuration { get; }
    public ITranslator Translator { get; }
    TwilioService Twilio { get; set; }

    public InviteUser(TwilioService twilio, 
        IRepository<Room> rooms, 
        IRepository<User> users, 
        PasswordlessAuthenticator<User> authenticator, 
        IConfiguration configuration,
        ITranslator translator)
    {
        Twilio = twilio;
        Rooms = rooms;
        Users = users;
        Authenticator = authenticator;
        Configuration = configuration;
        Translator = translator;
    }

    public override async Task<UserAvatar?> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).First();
            var invitingUser = await Users.GetAsync(User);
            var user = Users.Query.Where(u => u.Email == request.Email).FirstOrDefault();
            var exampleTranslation = "Your friend";

            if (user == null)
            {
                user = new(request.Email);
                if (request.Language != null)
                {
                    var language = await Translator.GetLanguageAsync(request.Language);
                    if (language != null)
                    {
                        user.ChangeVoice(language);
                        exampleTranslation = await Translator.TranslateAsync(exampleTranslation, "en", request.Language);
                    }
                }
                await Users.AddAsync(user);
            }

            string roomLink = await Authenticator.CreateMagicSignInLinkAsync(user, $"{Configuration["WebClientUrl"]}/rooms/{request.RoomId}");
            roomLink = $"{Request.Scheme}://{Request.Host.Value}{roomLink}";


            var templateData = new
            {
                RoomName = room.Name,
                InvitingUser = invitingUser?.Avatar.Name ?? exampleTranslation,
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
