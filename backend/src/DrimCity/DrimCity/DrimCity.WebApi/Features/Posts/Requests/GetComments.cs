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
                async Task<Results<Ok<CommentModel[]>, NotFound<ProblemDetails>>> (IMediator mediator, string slug,
                    CancellationToken cancellationToken) =>
                {
                    var comments = await mediator.Send(new Request(slug), cancellationToken);
                    return TypedResults.Ok(comments);
                });
        }
    }

    public record Request(string PostSlug) : IRequest<CommentModel[]>;

    public class RequestHandler : IRequestHandler<Request, CommentModel[]>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CommentModel[]> Handle(Request request, CancellationToken cancellationToken)
        {
            var comments = await _dbContext.Comments
                .Include(x => x.Post)
                .Where(x => x.Post.Slug == request.PostSlug)
                .Select(x => new CommentModel(x.Id, x.Content, x.CreatedAt, x.AuthorId))
                .ToArrayAsync(cancellationToken);

            return comments;
        }
    }
}