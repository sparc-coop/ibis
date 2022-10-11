using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features._Plugins;

[ApiExplorerSettings(IgnoreApi = true)]
public record SlackPost(string user_name, string text, string? command);
public record SlackParameters(string Command, string RoomId, string RoomSlug, string? Tag, string Text);

[ApiController]
[Route("slack")]
public class SlackApi : ControllerBase
{
    public IRepository<Room> Rooms { get; }
    public TypeMessage TypeMessage { get; }

    public SlackApi(IRepository<Room> rooms, TypeMessage typeMessage)
    {
        Rooms = rooms;
        TypeMessage = typeMessage;
    }

    [HttpPost("CreatePost")]
    public async Task<string> Post([FromForm] SlackPost request)
    {
        try
        {
            var parameters = ParseParameters(request.text);

            switch (parameters.Command)
            {
                case "post":
                    var message = await TypeMessage.ExecuteAsync(new(parameters.RoomId, parameters.Text, parameters.Tag));
                    return "\"" + (request.text.Length < 50 ? request.text + "\"" : $"{request.text[..50]}...\"")
                                + " posted to " + parameters.RoomSlug + (parameters.Tag != null ? "/" + parameters.Tag : "");
                default:
                    return $"Unknown command '{parameters.Command}'";
            }
        }
        catch (Exception e)
        {
            return $"Oops! {e.GetType().Name}: {e.Message}";
        }
    }

    SlackParameters ParseParameters(string text)
    {
        var parseRequest = text.Split(' ');
        var command = parseRequest[0];
        var slug = parseRequest[1];

        var channelId = Rooms.Query.FirstOrDefault(x => x.Slug == slug)?.Id;
        if (channelId == null)
            throw new NotFoundException($"Channel {slug} not found!");

        string? tagId = null;
        text = string.Join(' ', parseRequest.Skip(2));

        if (channelId.Contains('/'))
        {
            tagId = channelId.Split('/')[1];
            channelId = channelId.Split('/')[0];
        }

        return new(command, channelId, slug, tagId, text);
    }
}
