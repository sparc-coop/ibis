namespace Ibis.Messages;

public record TypeMessageWithTagsRequest(string RoomSlug, string Language, string Text, string? Tag = null, string? MessageId = null, string ContentType = "Text");
public class TypeMessageWithTags(IRepository<Message> messages, IRepository<Room> rooms)
{
    public IRepository<Message> Messages { get; } = messages;
    public IRepository<Room> Rooms { get; } = rooms;

    internal async Task<Message> ExecuteAsUserAsync(TypeMessageWithTagsRequest request, User user)
    {
        var room = (Rooms.Query.FirstOrDefault(x => x.Name == request.RoomSlug)
                    ?? Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug))
                    ?? throw new Exception("Room not found.");

        if (request.Tag != null)
        {
            var existingMessage = request.Tag != null
            ? Messages.Query.FirstOrDefault(x => x.RoomId == room.RoomId && x.Language == request.Language && x.Tag == request.Tag)
            : null;

            //var existingMessage =
            //    request.MessageId != null
            //    ? Messages.Query.FirstOrDefault(x => x.RoomId == room.RoomId && x.Id == request.MessageId)
            //    : Messages.Query.FirstOrDefault(x => x.RoomId == room.RoomId && x.Language == request.Language && x.Tag == request.Tag);

            if (existingMessage != null)
            {
                existingMessage.SetText(request.Text);
                existingMessage.SetHtmlFromMarkdown();
                await Messages.UpdateAsync(existingMessage);
                return existingMessage;
            }
        }

        // Saves the text as empty or null if it is a placeholder
        var message = new Message(room.RoomId, user!, request.Text ?? "", request.Tag ?? request.Text, contentType: request.ContentType);
        await Messages.AddAsync(message);
        return message;
    }
}
