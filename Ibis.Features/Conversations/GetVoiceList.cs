using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record GetVoiceListResponse(List<Voice> Voices);
    public class GetVoiceList : PublicFeature<string, GetVoiceListResponse>
    {
        public GetVoiceList(IbisEngine ibisEngine, List<Dialect> dialects)
        {
            IbisEngine = ibisEngine;
            Dialects = dialects;
        }

        public IbisEngine IbisEngine { get; }
        public List<Dialect> Dialects { get; set; }

        public override async Task<GetVoiceListResponse> ExecuteAsync(string locale)
        {
            var voices = await IbisEngine.GetVoicesForDialect(locale);
            var dialect = Dialects.Find(x => x.Locale == locale);
            if(dialect != null)
            {
                foreach (var item in voices)
                {
                    dialect.AddVoice(item.Locale, item.Name, item.DisplayName, item.LocaleName, item.ShortName, item.Gender, item.VoiceType);
                }
            }
            return new(voices);
        }
    }
}