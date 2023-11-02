using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Posts.Extensions;
using DrimCity.WebApi.Features.Posts.Models;
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
                (IMediator mediator, CancellationToken cancellationToken) =>
            {
                //todo: add pagination
                var posts = await mediator.Send(new Request(), cancellationToken);
                return TypedResults.Ok(posts);
            });
        }
    }

    public record Request : IRequest<IReadOnlyCollection<PostModel>>;

    public class RequestHandler : IRequestHandler<Request, IReadOnlyCollection<PostModel>>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<PostModel>> Handle(Request request, CancellationToken cancellationToken)
        {
            return await _dbContext.Posts
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PostModel(x.Id, x.Title, x.Content.Ellipsize(2000), x.CreatedAt, x.AuthorId, x.Slug))
                .ToListAsync(cancellationToken);
        }
    }
}
