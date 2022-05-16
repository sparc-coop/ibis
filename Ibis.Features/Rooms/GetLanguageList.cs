namespace Ibis.Features.Rooms;

public record GetLanguageListResponse(Dictionary<string, LanguageItem> Languages);
public class GetLanguageList : PublicFeature<GetLanguageListResponse>//List<string>>
{
    public GetLanguageList(IbisEngine ibisEngine)
    {
        IbisEngine = ibisEngine;
    }
    public IbisEngine IbisEngine { get; }

    public override async Task<GetLanguageListResponse> ExecuteAsync()
    {
        var languages = await IbisEngine.GetAllLanguages();
        var voices = await IbisEngine.GetAllVoices();

        var result = new Dictionary<string, LanguageItem>();
        foreach (var language in languages)
        {
            var voicesByDialect = voices
                .Where(x => x.Locale.Split("-").First() == language.Key)
                .GroupBy(x => x.Locale);

            var dialects = voicesByDialect
                .Select(x => new Dialect(language.Key, x.Key, x.First().LocaleName, x.ToList()))
                .ToList();

            result.Add(language.Key, language.Value with { Dialects = dialects });
        }

        return new(result);

    }
}
