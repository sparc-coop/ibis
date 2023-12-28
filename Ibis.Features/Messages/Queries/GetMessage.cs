namespace Ibis.Messages;

public record GetMessageRequest(string RoomSlug, string Tag, string? Language = null, bool AsHtml = false);
public class GetMessage
{
    public GetMessage(IRepository<Message> messages, IRepository<Room> rooms, ITranslator translator)
    {
        Messages = messages;
        Rooms = rooms;
        Translator = translator;
    }
    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }

    public async Task<Message> ExecuteAsync(GetMessageRequest request)
    {
        var room = Guid.TryParse(request.RoomSlug, out Guid roomId)
            ? Rooms.Query.FirstOrDefault(x => x.Id == request.RoomSlug)
            : Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug);

        if (room == null)
            throw new Exception($"Room {request.RoomSlug} not found");

        var originalMessage = Guid.TryParse(request.Tag, out Guid tagId)
            ? Messages.Query.FirstOrDefault(x => x.RoomId == room.Id && x.Id == request.Tag && x.SourceMessageId == null)
            : Messages.Query.FirstOrDefault(x => x.RoomId == room.Id && x.Tag == request.Tag && x.SourceMessageId == null);

        if (originalMessage == null)
            throw new Exception($"Message {request.Tag} not found!");

        if (request.Language == null || originalMessage.Language == request.Language)
            return originalMessage;

        var translation = await originalMessage.TranslateAsync(Translator, request.Language);
        if (translation.Item2 != null)
        {
            await Messages.AddAsync(translation.Item2);
            await Messages.UpdateAsync(originalMessage);
            return translation.Item2;
        }
        else
        {
            return Messages.Query.First(x => x.RoomId == room.Id && x.Id == translation.Item1!);
        }
    }
}
