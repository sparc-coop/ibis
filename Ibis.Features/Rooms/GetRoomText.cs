using System.Text;

namespace Ibis.Features.Rooms;

public partial class Rooms 
{
    public async Task<string> GetTextAsync(string id, string format)
    {
        var messages = await Messages.Query
            .Where(x => x.RoomId == id && x.SourceMessageId == null && x.Text != null)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        var builder = new StringBuilder();
        var num = 1;
        foreach (var message in messages)
        {
            switch (format.ToUpper())
            {
                case "TXT":
                    builder.AppendLine(message.Timestamp.ToString("MM/dd/yyyy hh:mm tt") +
                                    $": {message.Text}");
                    break;
                case "BRF":
                    builder.AppendLine(ToBrailleAscii(message.Text!));
                    break;
                case "SRT" when message.Audio != null:
                    builder.AppendLine(num++.ToString());
                    builder.AppendLine(
                          message.Timestamp.ToString("hh:mm:ss,ms") + " --> "
                        + message.Timestamp.AddTicks(message.Audio!.Duration).ToString("hh:mm:ss,ms"));
                    builder.AppendLine(message.Text);
                    builder.Append(Environment.NewLine);
                    break;
            }
        }

        return builder.ToString();
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

