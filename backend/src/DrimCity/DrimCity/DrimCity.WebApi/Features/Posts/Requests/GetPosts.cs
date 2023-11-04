using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Posts.Errors;
using DrimCity.WebApi.Features.Posts.Extensions;
using DrimCity.WebApi.Features.Posts.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class GetPosts
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapGet("/posts", async Task<Ok<IReadOnlyCollection<PostModel>>>
                (IMediator mediator, [AsParameters] QueryRequest queryRequest, CancellationToken cancellationToken) =>
            {
                var request = new Request(queryRequest.PageSize ?? 10, queryRequest.PageNumber ?? 1);
                var posts = await mediator.Send(request, cancellationToken);
                return TypedResults.Ok(posts);
            });
        }
    }

    public record QueryRequest(int? PageSize, int? PageNumber);

    public record Request(int PageSize, int PageNumber) : IRequest<IReadOnlyCollection<PostModel>>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.PageSize)
                .GreaterThanOrEqualTo(1).WithErrorCode(PostsValidationErrors.PageSizeMustBeGreaterOrEqualOne);

            RuleFor(request => request.PageNumber)
                .GreaterThanOrEqualTo(1).WithErrorCode(PostsValidationErrors.PageNumberMustBeGreaterOrEqualOne);
        }
    }

    public class RequestHandler : IRequestHandler<Request, IReadOnlyCollection<PostModel>>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<PostModel>> Handle(Request request, CancellationToken cancellationToken)
        {
            var pageIndex = request.PageNumber - 1;

            return await _dbContext.Posts
                .OrderByDescending(x => x.CreatedAt)
                .Skip(request.PageSize * pageIndex)
                .Take(request.PageSize)
                .Select(x => new PostModel(x.Id, x.Title, x.Content.Ellipsize(2000), x.CreatedAt, x.AuthorId, x.Slug))
                .ToListAsync(cancellationToken);
        }
    }
}
