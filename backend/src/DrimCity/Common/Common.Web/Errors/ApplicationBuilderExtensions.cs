using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Web.Errors.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Web.Errors;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MapProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>()!;
                var exception = exceptionHandlerPathFeature.Error;

                switch (exception)
                {
                    case ValidationErrorsException ex:
                    {
                        await WriteProblemDetailsToResponse(context,
                            "Validation failed",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
                            ex.Message,
                            StatusCodes.Status400BadRequest,
                            problemDetails =>
                            {
                                problemDetails.Extensions["errors"] = ex.Errors
                                    .Select(x => new ErrorData(x.Field, x.Message, x.Code));
                            });
                        break;
                    }
                    case LogicConflictException ex:
                        await WriteProblemDetailsToResponse(context,
                            "Logic conflict",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/409",
                            ex.Message,
                            StatusCodes.Status409Conflict,
                            problemDetails =>
                            {
                                problemDetails.Extensions["code"] = ex.Code;
                            });
                        break;
                    case OperationCanceledException:
                        await WriteProblemDetailsToResponse(context,
                            "Timeout",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/504",
                            "Request timed out",
                            StatusCodes.Status504GatewayTimeout);
                        break;
                    case InternalErrorException ex:
                        await WriteProblemDetailsToResponse(context,
                            "Internal error",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500",
                            ex.Message,
                            StatusCodes.Status500InternalServerError);
                        break;
                    default:
                        await WriteProblemDetailsToResponse(context,
                            "Internal error",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500",
                            "Interval server error has occured",
                            StatusCodes.Status500InternalServerError);
                        break;
                }
            });
        });

        return app;

        static async Task WriteProblemDetailsToResponse(HttpContext context, string title, string type, string detail,
            int statusCode, Action<ProblemDetails>? configureProblemDetails = null)
        {
            var problemDetails = new ProblemDetails
            {
                Title = title,
                Type = type,
                Detail = detail,
                Status = statusCode,
            };

            problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? context.TraceIdentifier);

            configureProblemDetails?.Invoke(problemDetails);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}

internal record ErrorData(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("code")] string Code);
