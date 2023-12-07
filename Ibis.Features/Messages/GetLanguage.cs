namespace Ibis.Messages;

public record GetLanguageRequest(string Text);
public class GetLanguage : PublicFeature<GetLanguageRequest, string>
{
    public GetLanguage(ITranslator translator, AzureLanguageDetector azureLanguageDetector)
    {
        AzureLanguageDetector = azureLanguageDetector;
        Translator = translator;
    }
    public ITranslator Translator { get; }
    public AzureLanguageDetector AzureLanguageDetector { get; }

    public override async Task<string> ExecuteAsync(GetLanguageRequest request)
    {
        var lang = AzureLanguageDetector.CallAzureLanguageDetector(request.Text);

        return lang;
    }
}
