using System.Globalization;

namespace Ibis.Features.Rooms;

public class Dialect
{
    public string Language { get; private set; }
    public string Locale { get; private set; }
    public string DisplayName { get; private set; }
    public string NativeName { get; private set; }
    public List<Voice> Voices { get; private set; }

    public Dialect(string locale)
    {
        var info = CultureInfo.GetCultureInfo(locale);

        Language = locale.Split('-').First();
        Locale = locale.Split('-').Last();
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