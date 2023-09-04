using System.Net.Http.Json;
using System.Text.Json;

namespace WebApi.Tests.Common.Helpers;

public static class HttpClientExtensions
{
    public static async Task<(TResponse?, HttpResponseMessage httpRespnse)> PostTyped<TResponse>(this HttpClient client, string url, object? body,
        CancellationToken cancellationToken)
    {
        var httpResponse = await client.PostAsJsonAsync(url, body, cancellationToken);

        var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<TResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return (response, httpResponse);
    }
}
