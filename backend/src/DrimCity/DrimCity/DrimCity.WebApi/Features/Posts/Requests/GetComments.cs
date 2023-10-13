using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Posts.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class GetComments
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapGet("/posts/{slug}/comments",
                async Task<Results<Ok<CommentModel[]>, NotFound, BadRequest<ProblemDetails>>> (IMediator mediator,
                    string slug, CancellationToken cancellationToken) =>
                {
                    var comments = await mediator.Send(new Request(slug), cancellationToken);

                    return comments is null
                        ? TypedResults.NotFound()
                        : TypedResults.Ok(comments);
                });
        }
    }

    public record Request(string PostSlug) : IRequest<CommentModel[]?>;

    public class RequestHandler : IRequestHandler<Request, CommentModel[]?>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CommentModel[]?> Handle(Request request, CancellationToken cancellationToken)
        {
            var post = await _dbContext.Posts
                .Where(post => post.Slug == request.PostSlug)
                .Select(post => new { post.Id })
                .SingleOrDefaultAsync(cancellationToken);

            if (post is null)
            {
                return null;
            }

            var comments = await _dbContext.Comments
                .Where(x => x.PostId == post.Id)
                .Select(x => new CommentModel(x.Id, x.Content, x.CreatedAt, x.AuthorId))
                .ToArrayAsync(cancellationToken);

            return comments;
        }
    }
}