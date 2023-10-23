using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Models;
using DrimCity.WebApi.Features.Posts.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using static DrimCity.WebApi.Features.Posts.Errors.PostsValidationErrors;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class CreatePost
{
    public class Endpoint : IEndpoint
    {
        private const string Path = "/posts";

        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Created<PostModel>>
                (IMediator mediator, Request request, CancellationToken cancellationToken) =>
            {
                var post = await mediator.Send(request, cancellationToken);
                return TypedResults.Created($"{Path}/{post.Slug}", post);
            });
        }
    }

    public record Request(string Title, string Content) : IRequest<PostModel>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithErrorCode(TitleRequired)
                .MaximumLength(Post.TitleMaxLength)
                    .WithMessage($"Title length must be less or equal than {Post.TitleMaxLength}")
                    .WithErrorCode(TitleMustBeLessOrEqualMaxLength);
            RuleFor(x => x.Content)
                .NotEmpty()
                    .WithErrorCode(ContentRequired)
                .MaximumLength(Post.ContentMaxLength)
                    .WithMessage($"Content length must be less or equal than {Post.ContentMaxLength}")
                    .WithErrorCode(ContentMustBeLessOrEqualMaxLength);
        }
    }

    public class RequestHandler : IRequestHandler<Request, PostModel>
    {
        private readonly AppDbContext _db;

        public RequestHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PostModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var slug = SlugGenerator.CreateSlug(request.Title);

            var post = new Post(0, request.Title, request.Content, DateTime.UtcNow, 1, slug);

            await _db.Posts.AddAsync(post, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new(post.Id, post.Title, post.Content, post.CreatedAt, post.AuthorId, post.Slug);
        }
    }
}
