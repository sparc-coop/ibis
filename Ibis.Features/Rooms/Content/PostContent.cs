using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;


public record PostContentRequest(string RoomSlug, string Language, Dictionary<string, string>? Messages = null, bool AsHtml = false);
public record GetAllContentResponse(string Name, string Slug, string Language, Dictionary<string, Message> Content);

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

        var content = await GetAllMessagesAsDictionaryAsync(request, room);        

        var response = new GetAllContentResponse(room.Name, room.Slug, request.Language, content);        

        return response;
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
        {            
            return;
        }

        foreach (var kvp in request.Messages)
        {
            var tag = kvp.Key?.Trim(); 
            var text = kvp.Value?.Trim();             

            if (!string.IsNullOrEmpty(tag) || !string.IsNullOrEmpty(text))
            {                
                var messageRequest = new TypeMessageRequest(
                    room.Slug,
                    request.Language,
                    text,
                    tag
                );

                var createdMessage = await TypeMessage.ExecuteAsUserAsync(messageRequest, Users.User.System);                
            }
            else
            {
                Console.WriteLine($"Ignoring message with empty Tag or Text. Tag = '{tag}', Text = '{text}'");
            }
        }        
    }
    
    private async Task<Dictionary<string, Message>> GetAllMessagesAsDictionaryAsync(PostContentRequest request, Room room)
    {       

        var language = request.Language ?? room.Languages.First().Id;

        var content = await Messages.Query(room.RoomId)
                    .Where(x => x.Language == language && x.Text != null)
                    .OrderBy(y => y.Timestamp)
                    .ToListAsync();                

        if (request.Messages != null && request.Messages.Count != 0)
        {
            content = content.Where(x => request.Messages.ContainsKey(x.Tag!)).ToList();            
        }

        var contentDictionary = content.ToDictionary(
            message => message.Tag!,
            message => message
        );
        
        return contentDictionary;
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
