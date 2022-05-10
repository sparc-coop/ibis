using Ibis.Features._Plugins;

namespace Ibis.Features.Conversations.Entities
{
    public class Dialect
    {
        public string Language { get; private set; }
        public string Locale { get; private set; }
        public string LocaleName { get; private set; }
        public List<Voice> Voices { get; private set; }

        private Dialect()
        { }

        public Dialect(string language, string locale, string localeName, List<Voice>? voices = null)
        {
            Language = language;
            Locale = locale;
            LocaleName = localeName;
            Voices = voices ?? new();
        }

        public void AddVoice(string locale, string name, string displayName, string localName, string shortName, string gender, string voiceType)
        {
            Voice voice = new(locale, name, displayName, localName, shortName, gender, voiceType);

            var existing = Voices.FindIndex(x => x.Name == name);

            if (existing == -1)
                Voices.Add(voice);
            else
                Voices[existing] = voice;
        }
    }
}