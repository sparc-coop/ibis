using Ibis.Messages.Queries;
using System.Text;

namespace Ibis.Messages;

public class Messages : BlossomRoot<Message>
{
    public Messages(IRepository<Message> messages) : base(messages)
    {
        Api.DeleteAsync = (Message m, User user) => m.Delete(user);
        Api.UpdateAsync = (Message m, string text) => m.SetText(text);
        Api.GetAllAsync = GetAll;
    }

    public async Task<IResult> GetAll(IRepository<Message> messages, IRepository<Room> rooms, IRepository<Language> languages, User? user, string id, HttpRequest request, int? take = null)
    {
        var room = await rooms.FindAsync(id);

        var language = await languages.FindAsync(new GetLanguage(request, room, user));
        var accept = request.GetTypedHeaders().Accept.OrderByDescending(x => x.Quality).FirstOrDefault();
        var format = accept?.MediaType == "text/plain" ? accept.Charset.Value : null;
        var result = await messages.GetAllAsync(new GetMessages(id, language!.Id, format, take));

        if (format != null)
        { 
            var text = string.Join("\r\n", result.Select(x => x.Text));
            var bytes = Encoding.UTF8.GetBytes(text);
            return Results.File(bytes, "text/plain");
        }

        return Results.Ok(result);
    }
}
