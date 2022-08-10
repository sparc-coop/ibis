namespace Ibis.Features.Messages;

public class Translation
{
    private Translation()
    {
        Language = "";
        Text = "";
    }

    public Translation(string language, string text)
    {
        Language = language;
        Text = text;
    }

    public string Language { get; private set; }
    public string Text { get; private set; }
    public string? ModifiedText { get; private set; }
    public string? AudioId { get; private set; }
    public double? Score { get; private set; }

    public void SetAudio(string audioId) => AudioId = audioId;

}
