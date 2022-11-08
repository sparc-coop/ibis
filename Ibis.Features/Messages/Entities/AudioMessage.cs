namespace Ibis.Features.Messages;

public class AudioMessage
{
    public AudioMessage()
    {
        Voice = "";
    }
    
    public AudioMessage(string? url, long duration, string voice, List<Word>? subtitles = null)
    {
        Url = url;
        Duration = duration;
        Voice = voice;
        Subtitles = subtitles;
    }

    public string? Url { get; private set; }
    public long Duration { get; private set; }
    public string Voice { get; private set; }
    public List<Word>? Subtitles { get; private set; }
}
