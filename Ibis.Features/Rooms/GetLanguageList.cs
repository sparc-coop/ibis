namespace Ibis.Features.Rooms;

public record GetLanguageListResponse(List<KeyValuePair<string, LanguageItem>> Languages);
public class GetLanguageList : PublicFeature<GetLanguageListResponse>//List<string>>
{
    public GetLanguageList(IbisEngine ibisEngine)
    {
        IbisEngine = ibisEngine;
    }
    public IbisEngine IbisEngine { get; }

    public override async Task<GetLanguageListResponse> ExecuteAsync()
    {
        var lanuages = await IbisEngine.GetAllLanguages();
        return new(lanuages);
    }
}
