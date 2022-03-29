using Ibis.Features._Plugins;
using Sparc.Features;

namespace Ibis.Features
{
    public record GetAudioToTextRequest(string Audiourl);
    public class AudioToText : PublicFeature<GetAudioToTextRequest, bool>
    {
        public AudioToText(IbisEngine ibisEngine)
        {
            IbisEngine = ibisEngine;    
        }

        public IbisEngine IbisEngine { get; }

        public override async Task<bool> ExecuteAsync(GetAudioToTextRequest userid)
        {
            var test = userid;
            await IbisEngine.RecognizeFromMic();
            return true;   
        }

    }
}
