namespace Ibis.Features.Rooms;

public class Voice
{
    public string Locale { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string LocaleName { get; private set; }
    public string ShortName { get; private set; }
    public string Gender { get; private set; }
    public string VoiceType { get; private set; }

    public Voice(string locale, string name, string displayName, string localeName, string shortName, string gender, string voiceType)
    {
        Locale = locale;
        Name = name;
        DisplayName = displayName;
        LocaleName = localeName;
        ShortName = shortName;
        Gender = gender;
        VoiceType = voiceType;
    }

    public Voice(Voice sourceVoice)
    {
        Locale = sourceVoice.Locale;
        Name = sourceVoice.Name;
        DisplayName = sourceVoice.DisplayName;
        LocaleName = sourceVoice.LocaleName;
        ShortName = sourceVoice.ShortName;
        Gender = sourceVoice.Gender;
        VoiceType = sourceVoice.VoiceType;
    }
}