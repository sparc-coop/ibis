using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

public record PostContentRequest(string RoomSlug, string Language, List<string>? Messages = null, bool AsHtml = false);
public record GetAllContentResponse(string Name, string Slug, string Language, List<Message> Content);

public class PostContent(IRepository<Message> messages, IRepository<Room> rooms, Translator translator, TypeMessage typeMessage)
{
    public IRepository<Message> Messages { get; } = messages;
    public IRepository<Room> Rooms { get; } = rooms;
    public Translator Translator { get; } = translator;
    public TypeMessage TypeMessage { get; } = typeMessage;

    public async Task<GetAllContentResponse> ExecuteAsync(PostContentRequest request)
    {
        var room = await GetRoomAsync(request.RoomSlug, null);
        await AddLanguageIfNeeded(room, request.Language);
        await TranslateMessagesAsync(request, room);

        var content = await GetAllMessagesAsync(request, room);

        return new(room.Name, room.Slug, request.Language, content);
    }

    private async Task AddAdditionalMessages(string roomSlug, List<string> additionalMessages)
    {
        foreach (var message in additionalMessages)
        {
            string contentType = GetContentTypeFromUrl(message);
            await TypeMessage.ExecuteAsUserAsync(new TypeMessageRequest(roomSlug, message, message, ContentType: contentType), Users.User.System);
        }
    }

    private static string GetContentTypeFromUrl(string message)
    {
        var contentType = "Text";

        if (Uri.TryCreate(message, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            var fileExtension = Path.GetExtension(uri.AbsolutePath);

            if (fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "Image";
            }
        }

        return contentType;
    }

    private async Task TranslateMessagesAsync(PostContentRequest request, Room room)
    {
        if (request.Messages == null || request.Messages.Count == 0)
            return;
        
        var messages = await Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        var untranslatedMessages = request.Messages.Where(x => !messages.Any(y => y.Tag == x)).ToList();
        if (untranslatedMessages.Count != 0)
            await AddAdditionalMessages(room.Slug, untranslatedMessages);
    }

    private async Task<List<Message>> GetAllMessagesAsync(PostContentRequest request, Room room)
    {
        var language = request.Language ?? room.Languages.First().Id;
        
        var content = await Messages.Query(room.RoomId)
                    .Where(x => x.Language == language && x.Text != null)
                    .OrderBy(y => y.Timestamp)
                    .ToListAsync();

        if (request.Messages != null && request.Messages.Count != 0)
            content = content.Where(x => request.Messages.Contains(x.Tag!)).ToList();

        return content;
    }

    private async Task AddLanguageIfNeeded(Room room, string languageId)
    {
        if (!room.Languages.Any(x => x.Id == languageId))
        {
            var language = await Translator.GetLanguageAsync(languageId);
            if (language == null)
                throw new Exception("Language not found!");

            room.AddLanguage(language);
            await Rooms.UpdateAsync(room);
        }
    }

    private async Task<Room> GetRoomAsync(string slug, User? user)
    {
        //await DeleteBadRoomsAsync(slug);
        
        var room = Rooms.Query.FirstOrDefault(x => x.Name == slug)
                    ?? Rooms.Query.FirstOrDefault(x => x.Slug == slug);

        if (room == null)
        {
            room = new Room(slug, "Content", user ?? Ibis.Users.User.System);
            await Rooms.AddAsync(room);
        }

        return room;
    }

    private async Task DeleteBadRoomsAsync(string slug)
    {
        var rooms = Rooms.Query.Where(x => x.Name == slug || x.Slug == slug).ToList();
        var ids = rooms.Select(x => x.RoomId).ToList();
        foreach (var badRoom in rooms)
        {
            var messages = Messages.Query
                .Where(x => x.RoomId == badRoom.Id)
                .ToList();
            await Messages.DeleteAsync(messages);
            Console.WriteLine($"Deleted {messages.Count} messages from room {badRoom.Name}.");
            await Rooms.DeleteAsync(badRoom);
        }
    }
}
