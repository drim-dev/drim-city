using System.Security.Claims;
using Common.Web.Auth;
using Common.Web.Endpoints;
using Common.Web.Validation.Extensions;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static DrimCity.WebApi.Features.Posts.Errors.PostsValidationErrors;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class CreateComment
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapPost("/posts/{slug}/comments", async Task<Results<Created<CommentModel>, NotFound>>
                (IMediator mediator, string slug, RequestBody body, ClaimsPrincipal user,
                    CancellationToken cancellationToken) =>
            {
                var request = new Request(user.GetUserId(), body.Content, slug);
                var comment = await mediator.Send(request, cancellationToken);

                if (comment is null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Created($"/posts/{slug}/comments/{comment.Id}", comment);
            })
                .RequireAuthorization();
        }

        private record RequestBody(string Content);
    }

    public record Request(int AuthorId, string Content, string Slug) : IRequest<CommentModel?>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty(CommentContentMustNotBeEmpty)
                .MaximumLength(Comment.ContentMaxLength, CommentContentMustBeLessOrEqualMaxLength);
        }
    }

    public class RequestHandler : IRequestHandler<Request, CommentModel?>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CommentModel?> Handle(Request request, CancellationToken cancellationToken)
        {
            var postId = await _dbContext.Posts
                .Where(x => x.Slug == request.Slug)
                .Select(x => (int?)x.Id)
                .SingleOrDefaultAsync(cancellationToken);

            if (postId is null)
            {
                return null;
            }

            var comment = new Comment(request.Content, DateTime.UtcNow, request.AuthorId, postId.Value);

            await _dbContext.Comments.AddAsync(comment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CommentModel(comment.Id, comment.Content, comment.CreatedAt, comment.AuthorId);
        }
    }
}
