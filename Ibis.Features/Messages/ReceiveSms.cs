//using Microsoft.EntityFrameworkCore;
//using Twilio.AspNet.Common;
//using Twilio.AspNet.Core;
//using Twilio.TwiML;

//namespace Ibis.Features.Messages;

//public class ReceiveSms : TwilioController
//{
//    public ReceiveSms(IRepository<User> users)
//    {
//        Users = users;
//    }

//    public IRepository<User> Users { get; }

//    public async Task<TwiMLResult> Index(SmsRequest incomingMessage)
//    {
//        // Find active room ID & user
//        var user = await Users.Query
//            .Where(x => x.PhoneNumber == incomingMessage.From)
//            .FirstOrDefaultAsync();

//        var roomId = user?.ActiveRooms
//            .OrderByDescending(x => x.JoinDate)
//            .FirstOrDefault()?.RoomId;

//        //if (user != null && roomId != null)
//        //{
//        //    var request = new SendMessageRequest(roomId, incomingMessage.Body, user.PrimaryLanguageId, null, null, SourceTypes.Sms, null);
//        //    await SendMessage.ExecuteAsync(request);
//        //}

//        MessagingResponse response = new();
//        return TwiML(response);
//    }
//}
