using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features._Plugins;

[ApiExplorerSettings(IgnoreApi = true)]
public record SlackPost(string team_id, string user_id, string text, string? command);
public record SlackParameters(string Command, string? RoomId, User? User, string Slug, string? Tag, string Text);

[ApiController]
[Route("slack")]
public class SlackApi : ControllerBase
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public TypeMessage TypeMessage { get; }
    public CreateRoom CreateRoom { get; }
    public GetRooms GetRooms { get; }

    public SlackApi(IRepository<Room> rooms, IRepository<User> users, TypeMessage typeMessage, CreateRoom createRoom, GetRooms getrooms)
    {
        Rooms = rooms;
        Users = users;
        TypeMessage = typeMessage;
        CreateRoom = createRoom;
        GetRooms = getrooms;
    }

    [HttpPost("")]
    public async Task<string> Post([FromForm] SlackPost request)
    {
        try
        {
            var parameters = ParseParameters(request);

            return parameters.Command switch
            {
                "login" => await LoginAsync(request, parameters),
                "post" => await PostAsync(parameters),
                "createroom" => await CreateRoomAsync(parameters),
                "listrooms" => await ListRoomsAsync(parameters),
                _ => $"Unknown command '{parameters.Command}'",
            };
        }
        catch (Exception e)
        {
            return $"Oops! {e.GetType().Name}: {e.Message} {e.InnerException?.Message} {e.StackTrace}";
        }
    }

    private async Task<string> ListRoomsAsync(SlackParameters parameters)
    {
        CheckLoggedIn(parameters);

        var rooms = await GetRooms.ExecuteAsUserAsync(parameters.User!);
        return string.Join("\r\n", rooms.Select(room => $"{room.Name} => {room.Slug} (Last activity: {room.LastActiveDate:d})"));
    }

    private async Task<string> CreateRoomAsync(SlackParameters parameters)
    {
        CheckLoggedIn(parameters);

        var roomName = parameters.Slug + (string.IsNullOrWhiteSpace(parameters.Text) ? "" : $" {parameters.Text}");
        var room = await CreateRoom.ExecuteAsUserAsync(new(roomName, "Content", new()), parameters.User!);
        return $"Room created! Use '/ibis post {room.Slug} [text]' to post to this room.";
    }

    private async Task<string> PostAsync(SlackParameters parameters)
    {
        if (parameters.RoomId == null)
            throw new NotFoundException($"Room {parameters.Slug} not found!");

        CheckLoggedIn(parameters);

        var message = await TypeMessage.ExecuteAsUserAsync(new(parameters.RoomId, parameters.Text, parameters.Tag), parameters.User!);

        return "\"" + (parameters.Text.Length < 50 ? parameters.Text + "\"" : $"{parameters.Text[..50]}...\"")
                    + " posted to " + parameters.Slug + (parameters.Tag != null ? "/" + parameters.Tag : "");
    }

    private static void CheckLoggedIn(SlackParameters parameters)
    {
        if (parameters.User == null)
            throw new NotAuthorizedException($"You're not logged in! Please log in first using /ibis login [userid]. You can find your User ID in Ibis's user settings.");
    }

    private async Task<string> LoginAsync(SlackPost request, SlackParameters parameters)
    {
        var user = await Users.FindAsync(parameters.Slug);
        if (user == null)
            throw new NotFoundException($"User ID {parameters.Slug} not found!");

        await Users.ExecuteAsync(user, u => u.RegisterWithSlack(request.team_id, request.user_id));
        return "You're logged in!";
    }

    SlackParameters ParseParameters(SlackPost request)
    {
        var parseRequest = request.text.Split(' ');
        var command = parseRequest[0];
        var slug = parseRequest.Length > 1 ? parseRequest[1] : "";

        var channelId = Rooms.Query.FirstOrDefault(x => x.Slug == slug)?.Id;

        var user = Users.Query
            .FirstOrDefault(x => x.SlackTeamId == request.team_id && x.SlackUserId == request.user_id);

        string? tagId = null;
        var text = string.Join(' ', parseRequest.Skip(2));

        if (channelId != null && channelId.Contains('/'))
        {
            tagId = channelId.Split('/')[1];
            channelId = channelId.Split('/')[0];
        }

        return new(command, channelId, user, slug, tagId, text);
    }
}
