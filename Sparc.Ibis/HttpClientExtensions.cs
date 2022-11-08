using System.Net.Http.Json;
using System.Text.Json;

namespace Sparc.Ibis;

public static class HttpClientExtensions
{
    public static async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(this HttpClient client, string url, TRequest request)
    {
        try
        {
            var response = await client.PostAsJsonAsync<TRequest>(url, request);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception e)
        {
            return default;
        }
    }
}