using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Conversations
{
    public record UploadFileRequest(string ConversationId, string Language, byte[] Bytes);

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
            var conversation = await Conversations.FindAsync(request.ConversationId);
            File file = new("speak", $"{request.ConversationId}/test.wav", AccessTypes.Public, new MemoryStream(request.Bytes));
            await Files.AddAsync(file);
            conversation.SetAudio(file.Url!);
            await Conversations.UpdateAsync(conversation);
            string url = file.Url!;

            return conversation;
        }
    }
}
