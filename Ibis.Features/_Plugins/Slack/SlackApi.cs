using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features._Plugins;

[ApiController]
[Route("api")]
public class SlackCommands : ControllerBase
{
    public IRepository<SlackPost> Posts { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }
    public SlackCommands(
        IRepository<SlackPost> posts, 
        IRepository<Room> rooms, 
        IRepository<Message> messages)
    {
        Posts = posts;
        Rooms = rooms;
        Messages = messages;
    }

    [HttpPost("CreatePost")]
    public async Task<string> Post([FromForm]SlackPost request)
    {
        bool success = await SaveNewPost(request);

        if (!success)
        {
            return "Oops! Please format the parameters correctly.";
        } else
        {
            return $"'{request.text}' - Thank you, your post has been received!";
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<bool> SaveNewPost(SlackPost request)
    {
        Room room = new();
        await Rooms.AddAsync(room);

        string[] parseRequest = request.text.Split(' ');
        string postType = parseRequest[0];
        string site = parseRequest[1];
        string tagId = "";
        request.text = string.Join(' ', parseRequest.Skip(2));

        if (site.Contains('/'))
        {
            site = site.Split('/')[0];
            tagId = site.Split('/')[1];
        }

        var message = new Message(room.RoomId, request.text);
        message.SiteName = site;
        if(!string.IsNullOrEmpty(tagId))
            message.Tag = tagId;
        await Messages.AddAsync(message);

        return true;
    }
}
