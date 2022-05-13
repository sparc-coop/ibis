using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;
using System.Collections;
using System.Text;

namespace Ibis.Features.Conversations
{
    public record BrailleRequest(string ConversationId);

    public class GetTextToBraille : PublicFeature<BrailleRequest, List<string>>
        {

            public IRepository<Message> Messages { get; }
            public IRepository<User> Users { get; }
            public IRepository<Conversation> Conversations { get; }
            public IbisEngine IbisEngine { get; }

            public Dictionary<string, string> dic = new Dictionary<string, string>();

            public Dictionary<char, string> dicU = new Dictionary<char, string>();

        public GetTextToBraille(IRepository<Message> messages, IRepository<User> users, IRepository<Conversation> conversations, IbisEngine ibisEngine)
            {
                Messages = messages;
                Users = users;
                Conversations = conversations;
                IbisEngine = ibisEngine;
            }

            public override async Task<List<string>> ExecuteAsync(BrailleRequest request)
            {


            BrailleAscii();
            var convo = await Conversations.FindAsync(request.ConversationId);        
            if (convo == null)
                throw new NotFoundException($"Conversation {request.ConversationId} not found!");

            var messages = Messages.Query
                .Where(x => x.ConversationId == request.ConversationId)
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
