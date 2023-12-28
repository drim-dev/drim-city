using Microsoft.Net.Http.Headers;
using RestSharp;

namespace DrimCity.WebApi.Tests.Extensions;

public static class RestClientHttpHeadersExtensions
{
    public static Uri? Location(this IReadOnlyCollection<HeaderParameter>? headers)
    {
        var header = headers?.SingleOrDefault(header => header.Name == HeaderNames.Location);
        var uriString = header?.Value?.ToString();
        return Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;
    }

    public static string? ContentType(this IReadOnlyCollection<HeaderParameter>? headers)
    {
        var header = headers?.SingleOrDefault(header => header.Name == HeaderNames.ContentType);
        return header?.Value?.ToString();
    }
}
