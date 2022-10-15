namespace Ibis.Features.Rooms;

public class Voice
{
    public string Locale { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string LocaleName { get; set; }
    public string ShortName { get; set; }
    public string Gender { get; set; }
    public string VoiceType { get; set; }

    public Voice()
    {
        Locale = "";
        Name = "";
        DisplayName = "";
        LocaleName = "";
        ShortName = "";
        Gender = "";
        VoiceType = "";
    }

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