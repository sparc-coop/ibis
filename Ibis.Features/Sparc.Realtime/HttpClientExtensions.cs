using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Sparc.Realtime;

public static class HttpClientExtensions
{
    public static async Task<TOut?> PostAsJsonAsync<TIn, TOut>(this HttpClient client, string url, TIn model)
    {
        var response = await client.PostAsJsonAsync(url, model);
        return await response.Content.ReadFromJsonAsync<TOut>();
    }
}