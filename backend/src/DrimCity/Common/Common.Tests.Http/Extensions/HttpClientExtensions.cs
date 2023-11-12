using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Tests.Http.Extensions;

public static class HttpClientExtensions
{
    //TODO: question: should we delete this class if we use RestSharp?
    public static async Task<(TResponse?, HttpResponseMessage httpResponse)> PostTyped<TResponse>(
        this HttpClient client, string url, object? body, CancellationToken cancellationToken)
    {
        var httpResponse = await client.PostAsJsonAsync(url, body, cancellationToken);

        return await GetDeserializedAndHttpResponse<TResponse>(httpResponse, cancellationToken);
    }

    public static async Task<(TResponse?, HttpResponseMessage httpResponse)> GetTyped<TResponse>(
        this HttpClient client, string url, CancellationToken cancellationToken)
    {
        var httpResponse = await client.GetAsync(url, cancellationToken);

        return await GetDeserializedAndHttpResponse<TResponse>(httpResponse, cancellationToken);
    }

    private static async Task<(TResponse? response, HttpResponseMessage httpResponse)>
        GetDeserializedAndHttpResponse<TResponse>(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            return (default, httpResponse);
        }

        var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseString))
        {
            return (default, httpResponse);
        }

        var response = JsonSerializer.Deserialize<TResponse>(
            responseString,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

        return (response, httpResponse);
    }
}
