namespace Ibis.Features.Conversations.Entities;

public class Translation
{
    private Translation()
    { }
    
    public Translation(string language, SourceTypes sourceType, string translation, double? score = null)
    {
        Language = language;
        SourceType = sourceType;
        Value = translation;
        Score = score;
    }

    public string Language { get; private set; }
    public SourceTypes SourceType { get; private set; }
    public string Value { get; private set; }
    public double? Score { get; private set; }
}
