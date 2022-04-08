using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;
using Sparc.Storage.Azure;
using File = Sparc.Storage.Azure.File;

namespace Ibis.Features.Conversations
{
    public record UploadFileRequest(string ConversationId, string Language, byte[] Bytes, string FileName);

    public class UploadFile : PublicFeature<UploadFileRequest, Message>
    {
        public IbisEngine IbisEngine { get; }
        public IRepository<Message> Messages { get; }
        public IRepository<User> Users { get; }
        public IRepository<File> Files { get; }

        public UploadFile(IbisEngine ibisEngine, IRepository<User> users, IRepository <Message> messages, IRepository<File> files)
        {
            IbisEngine = ibisEngine;
            Users = users;
            Messages = messages;
            Files = files;
        }

        public async override Task<Message> ExecuteAsync(UploadFileRequest request)
        {
            // create new message
            var user = await Users.FindAsync(User.Id());
            var message = new Message(request.ConversationId, User.Id(), request.Language ?? user!.PrimaryLanguageId, SourceTypes.Upload, "TestUserName", "AA");

            // upload file to Azure blob storage and set message.AudioId as file url
            // transcribes wav file to text and sets message.Text
            message = await IbisEngine.UploadFileAndTranscribe(message, request.Bytes, request.FileName);
            await Messages.AddAsync(message);

            return message;
        }
    }
}
