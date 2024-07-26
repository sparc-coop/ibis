using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Ibis.Rooms;
public record GetAllContentRequest(string RoomSlug, string? Language = null, Dictionary<string, string>? Tags = null, int? Take = null);
public record GetAllContentResponse(string Name, string Slug, string Language, List<Message> Content);
public class GetAllContent
{
    public GetAllContent(IRepository<Message> messages, IRepository<Room> rooms, Translator translator, TypeMessage typeMessage)
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

    public async Task<GetAllContentResponse> ExecuteAsync(GetAllContentRequest request)
    {
        var room = await GetRoomAsync(request.RoomSlug, null);
        var language = request.Language ?? room.Languages.First().Id;
        
        await AddLanguageIfNeeded(room, language);
        var result = await GetAllMessagesInUserLanguageAsync(request, room, language);

        return new(room.Name, room.Slug, language, result);
    }

    private async Task<List<Message>> GetAllMessagesInUserLanguageAsync(GetAllContentRequest request, Room room, string language)
    {
        IQueryable<Message> query = Messages.Query(room.RoomId)
                .Where(x => x.Language == language && x.Text != null)
                .OrderBy(y => y.Timestamp);

        if (request.Take != null)
            query = query.Take(request.Take.Value);

        List<Message> postList = await query.ToListAsync();

        // Optionally filter the content by tag
        if (request.Tags != null)
        {
            foreach (var tag in request.Tags.Keys)
            {
                postList = postList
                    .Where(x => x.Tags.Any(y => y.Key == tag && y.Value == request.Tags[tag]))
                    .ToList();
            }
        }

        return postList;
    }

    private async Task AddLanguageIfNeeded(Room room, string languageId)
    {
        if (!room.Languages.Any(x => x.Id == languageId))
        {
            var language = await Translator.GetLanguageAsync(languageId) 
                ?? throw new Exception("Language not found!");
            
            room.AddLanguage(language);
            await Rooms.UpdateAsync(room);

            var messages = await Messages.Query
                .Where(x => x.RoomId == room.RoomId && x.SourceMessageId == null)
                .ToListAsync();

            var translatedMessages = new ConcurrentBag<Message>();
            await Parallel.ForEachAsync(messages, async (message, token) =>
            {
                var result = await room.TranslateAsync(message, Translator);
                result.ForEach(x => translatedMessages.Add(x));
            });

            await Messages.AddAsync(translatedMessages);
            await Messages.UpdateAsync(messages);
        }
    }

    private async Task<Room> GetRoomAsync(string slug, User? user)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Name == slug)
                    ?? Rooms.Query.FirstOrDefault(x => x.Slug == slug);

        if (room == null)
        {
            room = new Room(slug, "Content", user ?? Ibis.Users.User.System);
            await Rooms.AddAsync(room);
        }

        return room;
    }

    
}
