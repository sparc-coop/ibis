namespace Ibis.Features.Conversations.Entities;

public class Translation
{
    private Translation()
    { }
    
    public Translation(string language)
    {
        Language = language;
    }

    public string Language { get; private set; }
    public string Text { get; private set; }
    public string? AudioId { get; private set; }
    public double? Score { get; private set; }

    public void SetText(string text) => Text = text;
    public void SetAudio(string audioId) => AudioId = audioId;

}
