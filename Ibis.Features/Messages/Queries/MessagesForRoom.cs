using Ardalis.Specification;

namespace Ibis.Features.Messages.Queries
{
    public class MessagesForRoom : Specification<Message>
    {
        public MessagesForRoom(string id, string language, bool? hasAudio = null)
        {
            Query
            .Where(x => x.RoomId == id && x.Language == language);

            if (hasAudio != null)
                Query.Where(x => hasAudio == (x.Audio != null));

            Query.OrderBy(x => x.Timestamp);
        }
    }
}
