using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record GetDialectListResponse(List<Dialect> Dialects);
    public class GetDialectList : PublicFeature<string, GetDialectListResponse>
    {
        public GetDialectList(IbisEngine ibisEngine, List<Language> languages)
        {
            IbisEngine = ibisEngine;
        }
        public IbisEngine IbisEngine { get; }
        public List<Language> Languages { get; set; }

        public override async Task<GetDialectListResponse> ExecuteAsync(string name)
        {
            var dialects = await IbisEngine.GetDialectsForLanguage(name);
            var language = Languages.Find(x => x.Name == name);
            if (language != null)
            {
                foreach(var item in dialects)
                {
                    language.AddDialect(item.Language, item.Locale, item.LocaleName);
                }
            }
            return new(dialects);
        }
    }
}