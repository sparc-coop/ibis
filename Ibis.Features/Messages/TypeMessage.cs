namespace Ibis.Messages;

public record TypeMessageRequest(string RoomSlug, string Language, string Text, string? Tag = null, string? MessageId = null, string ContentType = "Text", bool CreateNew = false);
public class TypeMessage(IRepository<Message> messages, IRepository<Room> rooms)
{
    public IRepository<Message> Messages { get; } = messages;
    public IRepository<Room> Rooms { get; } = rooms;

    internal async Task<Message> ExecuteAsUserAsync(TypeMessageRequest request, User user)
    {
        if (user.ABMode)
        {
            request = request with { CreateNew = true }; 
        }

        var room = (Rooms.Query.FirstOrDefault(x => x.Name == request.RoomSlug)
                    ?? Rooms.Query.FirstOrDefault(x => x.Slug == request.RoomSlug))
                    ?? throw new Exception("Room not found.");

        Message? existingMessage = null;
        
        if (!request.CreateNew && (request.Tag != null || request.MessageId != null))
        {
            // If a tag is passed in, edit the message if it exists
            existingMessage =
                request.MessageId != null
                ? Messages.Query.FirstOrDefault(x => x.RoomId == room.RoomId && x.Id == request.MessageId)
                : Messages.Query.FirstOrDefault(x => x.RoomId == room.RoomId && x.Language == request.Language && x.Tag == request.Tag);
            
            if (existingMessage != null)
            {
                //if (request.MessageId != null && existingMessage.User.Id != user.Id)
                //    throw new Exception("You are not permitted to edit another user's message.");

                existingMessage.SetText(request.Text);
                existingMessage.SetHtmlFromMarkdown();
                await Messages.UpdateAsync(existingMessage);
                return existingMessage;
            }
        }

        var existingTag = request.Tag ?? existingMessage?.Tag;

        var message = new Message(room.RoomId, user!, request.Text, existingTag ?? request.Text, contentType: request.ContentType);
        await Messages.AddAsync(message);
        return message;
    }
}
