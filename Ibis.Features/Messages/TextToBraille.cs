namespace Ibis.Features.Messages;

public record BrailleRequest(string RoomId);

public class TranslateToBraille : PublicFeature<BrailleRequest, List<string>>
{
    public IRepository<Message> Messages { get; }

    public TranslateToBraille(IRepository<Message> messages)
    {
        Messages = messages;
    }

    public override async Task<List<string>> ExecuteAsync(BrailleRequest request)
    {
        var messages = await Messages.Query
            .Where(x => x.RoomId == request.RoomId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        List<string> newMessages = new();
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

    protected string ToBrailleAscii(string txt) => BrailleAscii.Aggregate(txt, (result, s) => result.Replace(s.Key, s.Value));

    private static readonly Dictionary<string, string> BrailleAscii = new()
    {
            { "and", "&" },
            { "for", "=" },
            { "of", "(" },
            { "the", "!" },
            { "with", ")" },
            { "ing", "+" },

            { "ch", "*" },
            { "gh", "<" },
            { "sh", "%" },
            { "th", "?" },
            { "wh", ":" },
            { "ed", "$" },
            { "er", "]" },
            { "ou", "\\" },
            { "ow", "[" },
            { ",", "1" },
            { ";", "2" },
            { ":", "3" },
            { ".", "4" },
            { "en", "5" },
            { "!", "6" },
            { "(", "7" },
            { "?", "8" },
            { "in", "9" },
            { "'", "0" },
            { "st", "/" },
            { "#", "#" },
            { "ar", ">" },
            { " ", "(space)" }
        };
}

