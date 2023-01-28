using Ibis._Plugins;
using Microsoft.AspNetCore.Mvc;
using Sparc.Ibis;
using System.Text;

namespace Ibis.Rooms;

public record DownloadRoomContentRequest(string RoomId, string Format, string Language);
public class DownloadRoomContent : Feature<GetRoomTextRequest, FileResult>
{
    public DownloadRoomContent(IRepository<Messages.Message> messages, IRepository<Room> rooms, ITranslator translator, GetAllContent getAllContent)
    {
        Messages = messages;
        Rooms = rooms;
        Translator = translator;
        GetAllContent = getAllContent;
    }

    public IRepository<Messages.Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }
    public GetAllContent GetAllContent { get; }

    public override async Task<FileResult> ExecuteAsync([FromQuery]GetRoomTextRequest request)
    {
        var content = await GetAllContent.ExecuteAsync(new(request.RoomId, request.Language));

        var builder = new StringBuilder();
        var num = 1;
        foreach (var message in content.Content)
        {
            switch (request.Format.ToUpper())
            {
                case "TXT":
                    builder.AppendLine(message.User?.Name + " " + message.Timestamp.ToString("MM/dd/yyyy hh:mm tt") +
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

        var file = Encoding.UTF8.GetBytes(builder.ToString());
        var type = request.Format.ToUpper() == "TXT" ? "text/plain" : "application/octet-stream";
        return File(file, type, $"{content.Name} - {request.Language}.{request.Format.ToLowerInvariant()}");
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

