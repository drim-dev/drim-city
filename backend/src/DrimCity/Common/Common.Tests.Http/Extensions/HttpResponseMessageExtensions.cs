using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Tests.Http.Extensions;

public static class HttpResponseMessageExtensions
{
    //TODO: question: should we delete this class if we use RestSharp?
    public static async Task ShouldBeLogicConflictError(this HttpResponseMessage response, string message, string code)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Logic conflict");
        problemDetails.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/409");
        problemDetails.Detail.Should().Be(message);
        problemDetails.Extensions.Should().ContainKey("code");
        problemDetails.Extensions["code"]!.ToString().Should().Be(code);
    }
}
