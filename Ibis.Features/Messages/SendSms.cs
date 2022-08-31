using Sparc.Notifications.Twilio;

namespace Ibis.Features.Messages;

public class SendSms : BackgroundFeature<MessageTextChanged>
{
    public SendSms(IRepository<Room> rooms, TwilioService twilio)
    {
        Rooms = rooms;
        Twilio = twilio;
    }

    public IRepository<Room> Rooms { get; }
    public TwilioService Twilio { get; }

    public override async Task ExecuteAsync(MessageTextChanged notification)
    {
        var room = await Rooms.FindAsync(notification.RoomId);

        if (room == null || string.IsNullOrWhiteSpace(notification.Message.Text))
            return;

        var usersToSms = room!.ActiveUsers
            .Where(x => x.PhoneNumber != null && notification.Message.Language == x.Language)
            .ToList();

        foreach (var userToSms in usersToSms)
            await Twilio.SendSmsAsync(userToSms.PhoneNumber!, notification.Message.Text);
    }
}
