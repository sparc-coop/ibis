using System.Text.Json;

namespace Ibis._Plugins;

public static class HttpClientExtensions
{
    public static async Task<TResponse?> PostAsJsonAsync<TResponse>(this HttpClient client, string url, object request)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, request);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception e)
        {
            return default;
        }
    }

}
