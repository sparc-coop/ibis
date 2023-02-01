using System.Globalization;

namespace Ibis.Rooms;

public class Dialect
{
    public string Language { get; private set; }
    public string Locale { get; private set; }
    public string DisplayName { get; private set; }
    public string NativeName { get; private set; }
    public List<Voice> Voices { get; private set; }

    public Dialect()
    {
        Language = string.Empty;
        Locale = string.Empty;
        DisplayName = string.Empty;
        NativeName = string.Empty;
        Voices = new();
    }

    public Dialect(string localeName)
    {
        var info = CultureInfo.GetCultureInfo(localeName);

        Language = localeName.Split('-').First();
        Locale = localeName.Split('-').Last();
        DisplayName = info.DisplayName;
        NativeName = info.NativeName;
        Voices = new();
    }

    public void AddVoice(Voice voice)
    {
        var existing = Voices.FindIndex(x => x.ShortName == voice.ShortName);

        if (existing == -1)
            Voices.Add(voice);
        else
            Voices[existing] = voice;
    }
}