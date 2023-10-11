using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Tests.Http.Extensions;

public static class HttpClientExtensions
{
    public static async Task<(TResponse?, HttpResponseMessage httpResponse)> PostTyped<TResponse>(
        this HttpClient client, string url, object? body,
        CancellationToken cancellationToken)
    {
        var httpResponse = await client.PostAsJsonAsync(url, body, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            return (default, httpResponse);
        }

        var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<TResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return (response, httpResponse);
    }
}