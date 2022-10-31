using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Globalization;

namespace Sparc.Ibis;

public record GetAllContentRequest(string RoomSlug, string Language, List<string>? AdditionalMessages = null, bool AsHtml = false);
public record GetContentRequest(string RoomSlug, string Tag, string Language, bool AsHtml = false);
public record PostContentRequest(string RoomSlug, string Language, List<string> Messages, bool AsHtml = false);

public class IbisTranslator : IAsyncDisposable
{
    internal string Language { get; set; }
    internal List<IbisContent> Content { get; set; }
    private readonly Lazy<Task<IJSObjectReference>> IbisJs;

    internal HttpClient IbisClient;

    public IbisTranslator(IJSRuntime js, IConfiguration configuration)
    {
        var apiUrl = configuration["IbisApi"] ?? "https://api.ibis.chat";
        IbisClient = new HttpClient
        {
            BaseAddress = new Uri(apiUrl)
        };
        
        Content = new();
        IbisJs = new(() => js.InvokeAsync<IJSObjectReference>("import", "./_content/Sparc.Ibis/IbisTranslate.razor.js").AsTask());
        Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public async Task<string> InitAsync(ComponentBase component, string elementId, string? language = null)
    {
        var ibis = await IbisJs.Value;
        await ibis.InvokeVoidAsync("observe", elementId, DotNetObjectReference.Create(component));
        if (language != null)
            Language = language;

        return Language;
    }

    public async Task<IbisContent?> GetAsync(string channelId, string tag)
    {
        // some small caching
        if (this[tag] != null)
            return this[tag];

        var request = new GetContentRequest(channelId, tag, Language);
        return await IbisClient.PostAsJsonAsync<GetContentRequest, IbisContent>("/publicapi/GetContent", request);
    }

    public async Task<IbisChannel?> GetAllAsync(string channelId, List<string>? additionalMessages = null, bool asHtml = false)
    {
        var untranslatedMessages = additionalMessages?.Where(x => this[x] == null).ToList();
        var request = new GetAllContentRequest(channelId.ToLower(), Language, untranslatedMessages, asHtml);
        var response = await IbisClient.PostAsJsonAsync<GetAllContentRequest, IbisChannel>("/publicapi/GetAllContent", request);

        if (response != null)
        {
            // Cache the results
            foreach (var item in response.Content)
                this[item.Tag] = item;
        }

        return response;
    }

    public async Task<List<IbisContent>> PostAsync(string channelId, List<string> messages, bool asHtml = false)
    {
        var untranslatedMessages = messages.Where(x => this[x] == null).ToList();
        var request = new PostContentRequest(channelId.ToLower(), Language, untranslatedMessages, asHtml);
        var response = await IbisClient.PostAsJsonAsync<PostContentRequest, IbisChannel>("/publicapi/PostContent", request);

        if (response != null)
        {
            // Cache the results
            foreach (var item in response.Content)
                this[item.Tag] = item;
        }

        return messages
            .Where(x => this[x] != null)
            .Select(x => this[x]!)
            .ToList();
    }

    public async Task<List<string>> TranslateAsync(List<string> nodes, string channelId)
    {
        if (!nodes.Any())
            return nodes;
        
        var ibis = await IbisJs.Value;
        
        var nodesToTranslate = nodes.Where(x => !Content.Any(y => (y.Tag == x && y.Language == Language) || y.Text == x)).Distinct().ToList();

        await PostAsync(channelId, nodesToTranslate);

        // Replace nodes with their translation
        nodes = nodes.Select(x => this[x]?.Text ?? x).ToList();

        return nodes;
    }
    
    // This + IbisContent.ToString() enables use of @Ibis[tag] in Razor templates
    public IbisContent? this[string tag]
    {
        get { return Content.FirstOrDefault(x => x.Language == Language && x.Tag == tag); }
        set
        {
            if (value != null)
            {
                if (this[tag] != null)
                    Content.Remove(this[tag]!);

                Content.Add(value);
            }
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (IbisJs.IsValueCreated)
        {
            var module = await IbisJs.Value;
            await module.DisposeAsync();
        }
    }
}
