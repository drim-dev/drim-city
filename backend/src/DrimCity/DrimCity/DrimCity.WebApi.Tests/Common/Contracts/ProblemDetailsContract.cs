using System.Net;

namespace DrimCity.WebApi.Tests.Common.Contracts;

internal record ProblemDetailsContract(string? Title, HttpStatusCode Status, string? Detail);
