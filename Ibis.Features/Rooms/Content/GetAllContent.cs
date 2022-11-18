namespace Ibis.Features.Rooms;
public record GetAllContentRequest(string RoomSlug, string Language, bool AsHtml = false, Dictionary<string, string>? Tags = null, int? Take = null);
public record GetAllContentResponse(string Name, string Slug, List<GetContentResponse> Content);
public record GetContentResponse(string Tag, string Text, string Language, string? Audio, DateTime Timestamp, Dictionary<string, string> Tags);
public class GetAllContent : PublicFeature<GetAllContentRequest, GetAllContentResponse>
{
    public GetAllContent(IRepository<Message> messages, IRepository<Room> rooms, IRepository<User> users, ITranslator translator, TypeMessage typeMessage)
    {
        Messages = messages;
        Rooms = rooms;
        Users = users;
        Translator = translator;
        TypeMessage = typeMessage;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public ITranslator Translator { get; }
    public TypeMessage TypeMessage { get; }

    public override async Task<GetAllContentResponse> ExecuteAsync(GetAllContentRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        var room = await GetRoomAsync(request.RoomSlug, user);

        // Change the publish strategy so this call doesn't return until EVERYTHING is done
        ((Rooms as Sparc.Database.Cosmos.CosmosDbRepository<Room>)?.Context as SparcContext)?.SetPublishStrategy(PublishStrategy.ParallelWhenAll);

        await AddLanguageIfNeeded(room, request.Language);

        var result = await GetAllMessagesInUserLanguageAsync(request, room);

        return new(room.Name, room.Slug, result);
    }

    private async Task<List<GetContentResponse>> GetAllMessagesInUserLanguageAsync(GetAllContentRequest request, Room room)
    {
        IQueryable<Message> query = Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Text != null)
                    .OrderByDescending(y => y.Timestamp);

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

        List<GetContentResponse> result = new();
        foreach (var message in postList)
        {
            result.Add(new(
                message.Tag ?? message.Id, 
                request.AsHtml ? message.Html() : message.Text!, 
                message.Language, 
                message.Audio?.Url, 
                message.Timestamp,
                message.Tags.ToDictionary(x => x.Key, x => x.Value)));
        }

        return result;
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
            if (user != null)
            {
                room = new Room(slug, user);
                await Rooms.AddAsync(room);
            }
            else
            {
                throw new NotFoundException($"Room {slug} not found");
            }
        }

        return room;
    }
}
