using Ibis.Features._Plugins;
using Ibis.Features.Rooms;
using Sparc.Core;
using Sparc.Features;
using System.Collections;
using System.Text;

namespace Ibis.Features.Messages
{
    public record BrailleRequest(string RoomId);

    public class GetTextToBraille : PublicFeature<BrailleRequest, List<string>>
    {

        public IRepository<Message> Messages { get; }
        public IRepository<User> Users { get; }
        public IRepository<Room> Rooms { get; }
        public IbisEngine IbisEngine { get; }

        public Dictionary<string, string> dic = new Dictionary<string, string>();

        public GetTextToBraille(IRepository<Message> messages, IRepository<User> users, IRepository<Room> rooms, IbisEngine ibisEngine)
        {
            Messages = messages;
            Users = users;
            Rooms = rooms;
            IbisEngine = ibisEngine;
        }

        public override async Task<List<string>> ExecuteAsync(BrailleRequest request)
        {


            BrailleAscii();
            var convo = await Rooms.FindAsync(request.RoomId);
            if (convo == null)
                throw new NotFoundException($"Conversation {request.RoomId} not found!");

            var messages = Messages.Query
                .Where(x => x.RoomId == request.RoomId)
                .OrderBy(x => x.Timestamp)
                .ToList();

            List<string> newMessages = new List<string>();
            foreach (var msg in messages)
            {
                if (msg.Text != null)
                {
                    var newmsg = ToBrailleAscii(msg.Text);
                    newMessages.Add(newmsg);
                }

            }

            return newMessages;
        }
        ArrayList a1 = new ArrayList();
        public int String_Length;
        protected String ToBrailleAscii(string txt)
        {
            var translation = dic.Aggregate(txt, (result, s) => result.Replace(s.Key, s.Value));

            return translation;
        }
        private void BrailleAscii()
        {

            dic.Add("and", "&");
            dic.Add("for", "=");
            dic.Add("of", "(");
            dic.Add("the", "!");
            dic.Add("with", ")");
            dic.Add("ing", "+");

            dic.Add("ch", "*");
            dic.Add("gh", "<");
            dic.Add("sh", "%");
            dic.Add("th", "?");
            dic.Add("wh", ":");
            dic.Add("ed", "$");
            dic.Add("er", "]");
            dic.Add("ou", "\\");
            dic.Add("ow", "[");
            dic.Add(",", "1");
            dic.Add(";", "2");
            dic.Add(":", "3");
            dic.Add(".", "4");
            dic.Add("en", "5");
            dic.Add("!", "6");
            dic.Add("(", "7");
            dic.Add("?", "8");
            dic.Add("in", "9");
            dic.Add("'", "0");
            dic.Add("st", "/");
            dic.Add("#", "#");
            dic.Add("ar", ">");
            dic.Add(" ", "(space)");

        }
    }





}
