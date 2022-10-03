using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features._Plugins;

[ApiController]
[Route("api")]
public class SlackCommands : ControllerBase
{
    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }
    public SlackCommands(
        IRepository<Room> rooms, 
        IRepository<Message> messages)
    {
        Rooms = rooms;
        Messages = messages;
    }

    [HttpPost("CreatePost")]
    public async Task<string> Post([FromForm]SlackPost request)
    {
        var success = await SaveNewPost(request);

        if (string.IsNullOrEmpty(success))
        {
            return "Oops! Please format the parameters correctly.";
        } else
        {
            return $"Post successful! {success}";
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<string> SaveNewPost(SlackPost request)
    {
        try
        {
            string[] parseRequest = request.text.Split(' ');
            string postType = parseRequest[0];
            string site = parseRequest[1].ToLower();
            string tagId = "";
            request.text = string.Join(' ', parseRequest.Skip(2));

            if (site.Contains('/'))
            {
                tagId = site.Split('/')[1];
                site = site.Split('/')[0];
            }

            Room room = Rooms.Query.Where(x => x.Name == site).FirstOrDefault();

            if (room == null)
            {
                room = new(site);
                await Rooms.AddAsync(room);
            }

            Message message = new Message(room.RoomId, request.text);
            message.SiteName = site;

            if (!string.IsNullOrEmpty(tagId))
            {
                message = Messages.Query.Where(x => x.Tag == tagId).FirstOrDefault();

                if(message == null)
                {
                    message = new Message(room.RoomId, request.text);
                    message.Tag = tagId;                    
                    Messages.AddAsync(message);
                } else
                {
                    message.SetText(request.text);
                    Messages.UpdateAsync(message);
                }
            } else
            {
                Messages.AddAsync(message);
            }          

            return "\"" + (request.text.Length < 50 ? request.text + "\"" : $"{request.text.Substring(0, 50)}...\"") 
                + " posted to: " + site + (tagId != null ? ", " + tagId : "");
        } catch
        {
            return "";
        }

    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class SlackPost
    {
        public string? user_name { get; set; }
        public string? text { get; set; }
        public string? command { get; set; }

    }
}
