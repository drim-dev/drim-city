using RestSharp;

namespace DrimCity.WebApi.Tests.Extensions;

public static class RestClientExtensions
{
    public static async Task<RestResponse<TResponse>> ExecuteGetAsync<TResponse>(this IRestClient restClient,
        string url, CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest(url);
        return await restClient.ExecuteGetAsync<TResponse>(restRequest, cancellationToken);
    }

    public static async Task<RestResponse<TResponse>> ExecutePostAsync<TResponse>(this IRestClient restClient,
        string url, object request, CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest(url).AddJsonBody(request);
        return await restClient.ExecutePostAsync<TResponse>(restRequest, cancellationToken);
    }
}
