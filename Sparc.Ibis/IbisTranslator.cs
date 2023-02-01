using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace Sparc.Ibis;

public class IbisTranslator : IAsyncDisposable
{
    string? _language;
    internal string Language => _language ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    internal List<Message> Content { get; set; }
    private readonly Lazy<Task<IJSObjectReference>> IbisJs;

    internal IbisClient IbisClient;

    public IbisTranslator(IJSRuntime js, IbisClient client)
    {
        IbisClient = client;
        Content = new();
        IbisJs = new(() => js.InvokeAsync<IJSObjectReference>("import", "./_content/Sparc.Ibis/IbisContent.razor.js").AsTask());
    }

    public async Task<string> InitAsync(string channelId, string? language = null, List<Message>? restoredIbisContent = null)
    {
        _language = language;

        if (restoredIbisContent?.Any() == true)
            Content = restoredIbisContent;
        else
            await GetAllAsync(channelId, null);

        return Language;
    }

    public async Task InitClientAsync(ComponentBase component, string elementId)
    {
        var ibis = await IbisJs.Value;
        var content = Content.Where(x => x.Language == Language).ToDictionary(x => x.Tag, x => new
        {
            translation = x.Text,
            submitted = true,
            nodes = new List<object>(),

        });
        await ibis.InvokeVoidAsync("init", elementId, Language, DotNetObjectReference.Create(component), content);
    }

    public async Task<GetAllContentResponse?> GetAllAsync(string channelId, Dictionary<string, string>? tags = null, int? take = null)
    {
        var request = new GetAllContentRequest
        {
            RoomSlug = channelId,
            Language = Language,
            Tags = tags,
            Take = take
        };

        var response = await IbisClient.GetAllContentAsync(request);

        if (response != null)
        {
            // Cache the results
            foreach (var item in response.Content)
                Set(item.Tag, item);
        }

        return response;
    }

    public async Task<List<Message>> PostAsync(string channelId, List<string> messages, bool asHtml = false)
    {
        var untranslatedMessages = messages.Where(x => Get(x) == null).ToList();
        var request = new PostContentRequest
        {
            RoomSlug = channelId,
            Language = Language,
            Messages = untranslatedMessages,
            AsHtml = asHtml
        };

        var response = await IbisClient.PostContentAsync(request);

        if (response != null)
        {
            // Cache the results
            foreach (var item in response.Content)
                Set(item.Tag, item);
        }

        return messages
            .Select(x => Get(x)!)
            .Where(x => x != null)
            .ToList();
    }

    public async Task<List<string>> TranslateAsync(List<string> nodes, string channelId)
    {
        if (!nodes.Any())
            return nodes;

        var ibis = await IbisJs.Value;

        var nodesToTranslate = nodes.Where(x => !Content.Any(y => y.Language == Language && (y.Tag == x || y.Text == x))).Distinct().ToList();

        await PostAsync(channelId, nodesToTranslate);

        // Replace nodes with their translation
        nodes = nodes.Select(x => Get(x)?.Text ?? x).ToList();

        return nodes;
    }

    public async Task PlayAsync(Message message)
    {
        if (message?.Audio?.Url == null)
            return;
        
        var ibis = await IbisJs.Value;
        await ibis.InvokeVoidAsync("playAudio", message.Audio.Url);
    }

    // This + IbisContent.ToString() enables use of @Ibis[tag] in Razor templates
    public MarkupString this[string tag]
    {
        get
        {
            var content = Content.FirstOrDefault(x => x.Language == Language && x.Tag == tag);
            return content == null ? new("") : new(content.Text);
        }
    }

    public Message? Get(string tag) => Content.FirstOrDefault(x => x.Language == Language && x.Tag == tag);
    public void Set(string tag, Message value)
    {
        if (value != null)
        {
            var existing = Get(tag);
            if (existing != null)
                Content.Remove(existing!);

            Content.Add(value);
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
