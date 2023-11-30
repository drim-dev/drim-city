using System.Net;
using DrimCity.WebApi.Tests.Common.Contracts;
using RestSharp;

namespace DrimCity.WebApi.Tests.Extensions;

internal static class RestResponseExtensions
{
    public static void ShouldBeLogicConflictError(this RestResponse<ProblemDetailsContract> response, string message,
        string code)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Headers.ContentType()?.Should().Be("application/problem+json");

        var problemDetails = response.Data;
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.Should().Be(HttpStatusCode.Conflict);
        problemDetails.Title.Should().Be("Logic conflict");
        problemDetails.Type.Should().Be("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/409");
        problemDetails.Detail.Should().Be(message);
        problemDetails.Code.Should().Be(code);
    }
}
