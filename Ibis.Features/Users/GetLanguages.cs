using Microsoft.AspNetCore.OutputCaching;

namespace Ibis.Users;

[OutputCache(Duration = 3600)]
public class GetLanguages : PublicFeature<List<Language>>
{
    public GetLanguages(Translator translator, ISpeaker synthesizer)
    {
        Translator = translator;
        Synthesizer = synthesizer;
    }
    public Translator Translator { get; }
    public ISpeaker Synthesizer { get; }

    public override async Task<List<Language>> ExecuteAsync()
    {
        var languages = await Translator.GetLanguagesAsync();
        var voices = await Synthesizer.GetVoicesAsync();

        var result = new List<Language>();
        foreach (var language in languages)
        {
            var voicesByDialect = voices
                .Where(x => x.Locale.Split("-").First() == language.Id)
                .GroupBy(x => x.Locale);

            foreach (var dialect in voicesByDialect)
                language.AddDialect(dialect.Key, dialect.ToList());

            result.Add(language);
        }

        return result;

    }
}
