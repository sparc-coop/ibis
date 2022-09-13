namespace Ibis.Features.Messages;

public class AudioMessage
{
    public AudioMessage()
    { }
    
    public AudioMessage(string? url, long duration, Voice? voice = null, List<Word>? subtitles = null)
    {
        Url = url;
        Duration = duration;
        Voice = voice;
        Subtitles = subtitles;
    }

    public string? Url { get; private set; }
    public long Duration { get; private set; }
    public Voice? Voice { get; private set; }
    public List<Word>? Subtitles { get; private set; }
}
