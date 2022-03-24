using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record RecordSpeechFromMicRequeset(string Name);
    public class RecordSpeechFromMic : PublicFeature<RecordSpeechFromMicRequeset, Speech>
    {
        public IRepository<Speech> Speech { get; }
        public IbisEngine IbisEngine { get; }

        public RecordSpeechFromMic(IRepository<Speech> speech, IbisEngine ibisEngine)
        {
            Speech = speech;
            IbisEngine = ibisEngine;
        }

        public async override Task<Speech> ExecuteAsync(RecordSpeechFromMicRequeset request)
        {
            var speech =  await IbisEngine.RecognizeFromMic("test name");
            return speech;
        }
    }
}
