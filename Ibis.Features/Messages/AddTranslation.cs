namespace Ibis.Features.Messages;

public record AddTranslationRequest(string RoomId, string Language);
public class AddTranslation : Feature<AddTranslationRequest, List<Message>>
{
    public AddTranslation(IRepository<Message> messages, IbisEngine ibisEngine)
    {
        Messages = messages;
        IbisEngine = ibisEngine;
    }

    public IRepository<Message> Messages { get; }
    public IbisEngine IbisEngine { get; }

    public override async Task<List<Message>> ExecuteAsync(AddTranslationRequest request)
    {
        var untranslatedMessages = await Messages.Query
            .Where(x => x.RoomId == request.RoomId).ToListAsync();

        foreach (var message in untranslatedMessages.Where(x => !x.HasTranslation(request.Language)))
        {
            Message translatedMessage = await IbisEngine.TranslateAsync(message, request.Language);
            await Messages.UpdateAsync(translatedMessage);
        }

        return Messages.Query.Where(x => x.RoomId == request.RoomId).ToList();
    }
}
