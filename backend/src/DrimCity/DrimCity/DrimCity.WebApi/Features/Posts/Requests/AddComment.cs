using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class AddComment
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapPost("/posts/{slug}/comments",
                async Task<Results<Created<CommentModel>, BadRequest<ProblemDetails>>>
                (IMediator mediator, Request request, CancellationToken cancellationToken, string slug) =>
            {
                request = request with { Slug = slug }; //todo I think it is not okay to modify request like this
                var comment = await mediator.Send(request, cancellationToken);
                return TypedResults.Created($"/posts/{slug}/comments/{comment.Id}", comment);
            });
        }
    }

    public record Request(string Content) : IRequest<CommentModel>
    {
        public string Slug { get; init; } = null!;
    }

    public class RequestValidator : AbstractValidator<Request>
    {
    }

    public class RequestHandler : IRequestHandler<Request, CommentModel>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CommentModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var postId = await _dbContext.Posts
                .Where(x => x.Slug == request.Slug)
                .Select(x => x.Id)
                .SingleAsync(cancellationToken);

            var comment = new Comment(request.Content, DateTime.UtcNow, 1, postId);

            await _dbContext.Comments.AddAsync(comment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CommentModel(comment.Id, comment.Content, comment.CreatedAt, comment.AuthorId);
        }
    }
}