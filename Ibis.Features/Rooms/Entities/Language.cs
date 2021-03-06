using System.Globalization;

namespace Ibis.Features;

public class Language
{
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string NativeName { get; private set; }
    public bool IsRightToLeft { get; private set; }
    public List<Dialect> Dialects { get; private set; }

    private Language()
    {
    }

    public Language(string name)
    {
        var culture = CultureInfo.GetCultureInfo(name);

        Name = name;
        DisplayName = culture.DisplayName;
        NativeName = culture.NativeName;
        IsRightToLeft = culture.TextInfo.IsRightToLeft;
    }

    public Language(string name, string displayName, string nativeName, bool isRightToLeft)
    {
        Name = name;
        DisplayName = displayName;
        NativeName = nativeName;
        IsRightToLeft = isRightToLeft;
    }

    public void AddDialect(string language, string locale, string localeName)
    {
        Dialect dialect = new(language, locale, localeName);

        var existing = Dialects.FindIndex(x => x.Locale == locale);

        if (existing == -1)
            Dialects.Add(dialect);
        else
            Dialects[existing] = dialect;
    }
}