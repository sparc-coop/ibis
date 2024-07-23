using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

public record PostContentRequest(string RoomSlug, string Language, List<string> Messages, bool AsHtml = false);
public class PostContent
{
    public PostContent(IRepository<Message> messages, IRepository<Room> rooms, Translator translator, TypeMessage typeMessage)
    {
        Messages = messages;
        Rooms = rooms;
        Translator = translator;
        TypeMessage = typeMessage;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public Translator Translator { get; }
    public TypeMessage TypeMessage { get; }

    public async Task<GetAllContentResponse> ExecuteAsync(PostContentRequest request)
    {
        var room = await GetRoomAsync(request.RoomSlug, null);

        await AddLanguageIfNeeded(room, request.Language);

        var untranslatedMessages = await GetUntranslatedMessagesAsync(request, room);
        if (untranslatedMessages.Count != 0)
        {
            await AddAdditionalMessages(room.Id, untranslatedMessages);
        }

        var result = await GetAllMessagesAsync(request, room);

        return new(room.Name, room.Slug, request.Language, result);
    }

    private async Task AddAdditionalMessages(string roomId, List<string> additionalMessages)
    {
        foreach (var message in additionalMessages)
            await TypeMessage.ExecuteAsUserAsync(new TypeMessageRequest(roomId, message, message), Users.User.System);
    }

    private async Task<List<string>> GetUntranslatedMessagesAsync(PostContentRequest request, Room room)
    {
        var messages = await Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        var untranslatedMessages = request.Messages.Where(x => !messages.Any(y => y.Tag == x)).ToList();
        return untranslatedMessages;
    }

    private async Task<List<Message>> GetAllMessagesAsync(PostContentRequest request, Room room)
    {
        List<Message> postList = await Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        postList = postList.Where(x => request.Messages.Contains(x.Tag!)).ToList();

        return postList;
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
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == slug);
        if (room == null)
        {
            room = new Room(slug, "Content", user ?? Ibis.Users.User.System);
            await Rooms.AddAsync(room);
        }
        
        return room;
    }
}
