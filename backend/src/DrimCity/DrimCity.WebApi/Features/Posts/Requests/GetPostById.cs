using DrimCity.WebApi.Common.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Posts.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static DrimCity.WebApi.Features.Posts.Errors.PostsValidationErrors;

namespace DrimCity.WebApi.Features.Posts.Requests;

public static class GetPostById
{
    public class Endpoint : IEndpoint
    {
        private const string path = "/posts/get-post-by-id";
        public void MapEndpoint(WebApplication app)
        {
            
        }
    }

    public record Request(int Id):IRequest<PostModel>;

    public class RequestValidator:AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext appDbContext)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithErrorCode(IdRequired)
                .MustAsync(async (x, token) =>
                {
                    var idExists = await appDbContext.Posts.AnyAsync(post => post.Id == x, token);
                    return idExists;
                }).WithErrorCode(IdNotExists);
        }
    }

    public class RequestHandler : IRequestHandler<Request, PostModel>
    {
        private readonly AppDbContext _appDbContext;
        public RequestHandler(AppDbContext appDbContext) 
        {
            _appDbContext = appDbContext;
        }

        public async Task<PostModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var post = await _appDbContext.Posts.SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            return new(post.Id, post.Title, post.Content, post.CreatedAt, post.AuthorId, post.Slug);
        }
    }
}
    
