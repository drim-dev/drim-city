using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Posts.Errors;
using DrimCity.WebApi.Features.Posts.Extensions;
using DrimCity.WebApi.Features.Posts.Models;
using FluentValidation;
using MediatR;
using MessagePack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class GetPosts
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapGet("/posts", async Task<Ok<BodyResponse>>
                (IMediator mediator, [AsParameters] QueryRequest queryRequest, CancellationToken cancellationToken) =>
            {
                var request = CreateRequest(queryRequest, cancellationToken);

                var response = await mediator.Send(request, cancellationToken);

                var bodyResponse = CreateResponse(response, cancellationToken);

                return TypedResults.Ok(bodyResponse);
            });
        }

        private static Request CreateRequest(QueryRequest queryRequest, CancellationToken cancellationToken)
        {
            var pageToken = DeserializePageToken(queryRequest.PageToken, cancellationToken);
            var pageSize = queryRequest.PageSize switch
            {
                null or 0 => Request.DefaultPageSize,
                > 1000 => Request.MaximumPageSize,
                _ => queryRequest.PageSize.Value,
            };

            return new Request(pageSize, pageToken);
        }

        private static PageToken? DeserializePageToken(string? pageTokenAsString, CancellationToken cancellationToken)
        {
            if (pageTokenAsString == null)
            {
                return null;
            }

            var pageTokenAsBytes = Convert.FromBase64String(pageTokenAsString);
            var pageToken =
                MessagePackSerializer.Deserialize<PageToken>(pageTokenAsBytes.AsMemory(), null, cancellationToken);

            return pageToken;
        }

        private static BodyResponse CreateResponse(Response response, CancellationToken cancellationToken)
        {
            var nextPageTokenAsString = SerializePageToken(response.NextPageToken, cancellationToken);
            var bodyResponse = new BodyResponse(response.Posts, nextPageTokenAsString);
            return bodyResponse;
        }

        private static string? SerializePageToken(PageToken? pageToken, CancellationToken cancellationToken)
        {
            if (pageToken == null)
            {
                return null;
            }

            var pageTokenAsBytes = MessagePackSerializer.Serialize(pageToken, null, cancellationToken);
            var pageTokenAsString = Convert.ToBase64String(pageTokenAsBytes);

            return pageTokenAsString;
        }
    }

    public record QueryRequest(int? PageSize, string? PageToken)
    {
        /// <summary>
        ///     The maximum number of posts to return.
        ///     If unspecified or specified as 0, 10 posts will be returned.
        ///     The maximum value is 1000; values above 1000 will be coerced to 1000.
        /// </summary>
        public int? PageSize { get; init; } = PageSize;
    }

    public record Request(int PageSize, PageToken? PageToken) : IRequest<Response>
    {
        public const int DefaultPageSize = 10;
        public const int MaximumPageSize = 1000;
    }

    public record Response(PostModel[] Posts, PageToken? NextPageToken);

    public record BodyResponse(PostModel[] Posts, string? NextPageToken);

    [MessagePackObject]
    public record PageToken([property: Key(0)] int Skip);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.PageSize)
                .GreaterThanOrEqualTo(1).WithErrorCode(PostsValidationErrors.PageSizeMustBePositive);
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var posts = await _dbContext.Posts
                .OrderByDescending(x => x.CreatedAt)
                .Skip(request.PageToken?.Skip ?? 0)
                .Take(request.PageSize + 1)
                .Select(x => new PostModel(x.Id, x.Title, x.Content.Ellipsize(2000), x.CreatedAt, x.AuthorId, x.Slug))
                .ToArrayAsync(cancellationToken);

            var (actualPageSize, pageToken) = posts.Length > request.PageSize
                ? (request.PageSize, new PageToken(request.PageSize))
                : (posts.Length, null);

            return new Response(posts[..actualPageSize], pageToken);
        }
    }
}
