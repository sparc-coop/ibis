using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;
public record GetAllContentRequest(string RoomSlug, string Language, List<string>? AdditionalMessages = null);
public record GetAllContentResponse(string Name, string Slug, string Language, List<GetContentResponse> Content);
public record GetContentResponse(string Tag, string Text, string? Audio, DateTime Timestamp);
public class GetAllContent : PublicFeature<GetAllContentRequest, GetAllContentResponse>
{
    public GetAllContent(IRepository<Message> messages, IRepository<Room> rooms, ITranslator translator)
    {
        Messages = messages;
        Rooms = rooms;
        Translator = translator;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }

    public override async Task<GetAllContentResponse> ExecuteAsync(GetAllContentRequest request)
    {
        var room = GetRoom(request.RoomSlug);
        await AddLanguageIfNeeded(room, request.Language);

        var result = await GetAllMessagesAsync(request, room);

        if (request.AdditionalMessages?.Any() == true)
            await AddAdditionalMessages(request.AdditionalMessages);

        return new(room.Name, room.Slug, request.Language, result);
    }

    private async Task AddAdditionalMessages(List<string> additionalMessages)
    {
        // Don't do anything yet
    }

    private async Task<List<GetContentResponse>> GetAllMessagesAsync(GetAllContentRequest request, Room room)
    {
        List<Message> postList = await Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        List<GetContentResponse> result = new();
        foreach (var item in postList)
        {
            result.Add(new(item.Tag ?? item.Id, item.Text!, item.Audio?.Url, item.Timestamp));
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

    private Room GetRoom(string slug)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == slug);
        if (room == null)
        {
            throw new NotFoundException($"Room {slug} not found");
        }

        return room;
    }
}
