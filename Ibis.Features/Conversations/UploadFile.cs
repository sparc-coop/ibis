using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Conversations
{
    public record UploadFileRequest(string ConversationId, string Language, System.IO.FileStream FileStream);

    public class UploadFile : PublicFeature<UploadFileRequest, Conversation>
    {
        public IbisEngine IbisEngine { get; }
        public IRepository<Conversation> Conversations { get; }
        public IRepository<User> Users { get; }
        public IRepository<File> Files { get; }

        public UploadFile(IbisEngine ibisEngine, IRepository<Conversation> conversations, IRepository<File> files)
        {
            IbisEngine = ibisEngine;
            Conversations = conversations;
            Files = files;
        }

        public async override Task<Conversation> ExecuteAsync(UploadFileRequest request)
        {
            var user = await Users.FindAsync(User.Id());
            var conversation = await Conversations.FindAsync(request.ConversationId);
            await IbisEngine.UploadFile(conversation, request.Language, request.FileStream);
            return conversation;
        }
    }
}
