namespace Ibis;

public class Language
{
    public string Id { get; private set; }
    public string DisplayName { get; private set; }
    public string NativeName { get; private set; }
    public bool? IsRightToLeft { get; private set; }
    public List<Dialect> Dialects { get; private set; }

    public Language(string id, string displayName, string nativeName, bool? isRightToLeft)
    {
        Id = id.Split("-").First();
        DisplayName = displayName;
        NativeName = nativeName;
        IsRightToLeft = isRightToLeft;
        Dialects = [];

        if (id.Contains('-'))
        {
            AddDialect(id);
            DisplayName = DisplayName.Split('(').First().Trim();
            NativeName = NativeName.Split('(').First().Trim();
        }
    }

    public void AddDialect(string locale, List<Voice>? voices = null)
    {
        Dialect dialect = new(locale);
        if (voices != null)
            foreach (var voice in voices)
                dialect.AddVoice(voice);

        var existing = Dialects.FindIndex(x => x.Locale == dialect.Locale);

        if (existing == -1)
            Dialects.Add(dialect);
        else
            Dialects[existing] = dialect;
    }
}