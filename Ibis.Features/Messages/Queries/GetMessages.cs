using Ardalis.Specification;

namespace Ibis.Messages.Queries;

public class GetMessages : Specification<Message>
{
    public GetMessages(string roomId, string language, string? format = null, int? take = null)
    {
        Query.Where(x => x.RoomId == roomId)
            .Where(x => x.Language == language && x.Text != null)
            .OrderBy(y => y.Timestamp);

        if (take != null)
            Query.Take(take.Value);

        if (format != null)
            Query.PostProcessingAction(messages => {
                var firstMessageTimestamp = messages.FirstOrDefault(x => x.Audio != null)?.Timestamp;

                foreach (var message in messages)
                    switch (format)
                    {
                        case "srt":
                            message.ToSubtitles(firstMessageTimestamp!.Value);
                            break;
                        case "brf":
                            message.ToBrailleAscii();
                            break;
                        default:
                            message.ToText();
                            break;
                    }

                return messages;
            });
    }
}
