namespace Ibis.Features.Messages;
public record GetAllContentRequest(string RoomSlug, string Language, List<string>? AdditionalMessages = null, bool AsHtml = false);
public record GetAllContentResponse(string Name, string Slug, List<GetContentResponse> Content);
public record GetContentResponse(string Tag, string Text, string Language, string? Audio, DateTime Timestamp);
public class GetAllContent : PublicFeature<GetAllContentRequest, GetAllContentResponse>
{
    public GetAllContent(IRepository<Message> messages, IRepository<Room> rooms, ITranslator translator, TypeMessage typeMessage)
    {
        Messages = messages;
        Rooms = rooms;
        Translator = translator;
        TypeMessage = typeMessage;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }
    public TypeMessage TypeMessage { get; }

    public override async Task<GetAllContentResponse> ExecuteAsync(GetAllContentRequest request)
    {
        var room = GetRoom(request.RoomSlug);
        await AddLanguageIfNeeded(room, request.Language);

        var result = await GetAllMessagesAsync(request, room);

        if (request.AdditionalMessages?.Any() == true)
        {
            request.AdditionalMessages.RemoveAll(x => result.Any(y => y.Tag == x));
            await AddAdditionalMessages(room.Id, request.AdditionalMessages);
        }

        return new(room.Name, room.Slug, result);
    }

    private async Task AddAdditionalMessages(string roomId, List<string> additionalMessages)
    {
        foreach (var message in additionalMessages)
            await TypeMessage.ExecuteAsUserAsync(new(roomId, message, message), Users.User.System);
    }

    private async Task<List<GetContentResponse>> GetAllMessagesAsync(GetAllContentRequest request, Room room)
    {
        List<Message> postList = await Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        List<GetContentResponse> result = new();
        foreach (var message in postList)
        {
            result.Add(new(message.Tag ?? message.Id, request.AsHtml ? message.Html() : message.Text!, message.Language, message.Audio?.Url, message.Timestamp));
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
