using System.Text;

namespace Ibis.Rooms;

public record GetRoomTextRequest(string RoomId, string Format);
public record GetRoomTextResponse(string Text);
public class GetRoomText : Feature<GetRoomTextRequest, GetRoomTextResponse>
{
    public GetRoomText(IRepository<Message> messages)
    {
        Messages = messages;
    }

    public IRepository<Message> Messages { get; }

    public override async Task<GetRoomTextResponse> ExecuteAsync(GetRoomTextRequest request)
    {
        var messages = await Messages.Query
            .Where(x => x.RoomId == request.RoomId && x.SourceMessageId == null && x.Text != null)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        var builder = new StringBuilder();
        var num = 1;
        foreach (var message in messages)
        {
            if (request.Format.ToUpper() == "TXT")
            {
                builder.AppendLine(message.Timestamp.ToString("MM/dd/yyyy hh:mm tt") +
                    $": {message.Text}");
            }
            else if (request.Format.ToUpper() == "BRF")
            {
                builder.AppendLine(ToBrailleAscii(message.Text!));
            }
            else if (request.Format.ToUpper() == "SRT" && message.Audio != null)
            {
                builder.AppendLine(num++.ToString());
                builder.AppendLine(
                      message.Timestamp.ToString("hh:mm:ss,ms") + " --> "
                    + message.Timestamp.AddTicks(message.Audio!.Duration).ToString("hh:mm:ss,ms"));
                builder.AppendLine(message.Text);
                builder.Append(Environment.NewLine);
            }
        }

        return new(builder.ToString());
    }

    protected string ToBrailleAscii(string txt) => BrailleAscii.Aggregate(txt, (result, s) => result.Replace(s.Key, s.Value));
    protected string ToBrailleUnicode(string txt) => BrailleUnicode.Aggregate(txt, (result, s) => result.Replace(s.Key.ToString(), s.Value));

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

    private static readonly Dictionary<char, string> BrailleUnicode = new()
        {
            { 'a', "⠁" },
            { 'b', "⠃" },
            { 'c', "⠉" },
            { 'd', "⠙" },
            { 'e', "⠑" },
            { 'f', "⠋" },
            { 'g', "⠛" },
            { 'h', "⠓" },
            { 'i', "⠊" },
            { 'j', "⠚" },
            { 'k', "⠅" },
            { 'l', "⠇" },
            { 'm', "⠍" },
            { 'n', "⠝" },
            { 'o', "⠕" },
            { 'p', "⠏" },
            { 'q', "⠟" },
            { 'r', "⠗" },
            { 's', "⠎" },
            { 't', "⠞" },
            { 'u', "⠥" },
            { 'v', "⠧" },
            { 'w', "⠺" },
            { 'x', "⠭" },
            { 'y', "⠽" },
            { 'z', "⠵" },
            { ' ', "  " }
        };
}

