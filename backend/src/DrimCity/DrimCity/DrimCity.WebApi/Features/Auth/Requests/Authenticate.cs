using Common.Web.Endpoints;
using Common.Web.Errors.Exceptions;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Auth.Models;
using DrimCity.WebApi.Features.Auth.Services;
using DrimCity.WebApi.Features.Auth.Validation;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Features.Auth.Requests;

// TODO: add refresh token
public static class Authenticate
{
    public class Endpoint : IEndpoint
    {
        private const string Path = "/auth";

        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Results<Ok<TokenModel>, BadRequest<ProblemDetails>, UnauthorizedHttpResult>>
                (IMediator mediator, Request request, CancellationToken cancellationToken) =>
            {
                var token = await mediator.Send(request, cancellationToken);
                return TypedResults.Ok(token);
            });
        }
    }

    public record Request(string Login, string Password) : IRequest<TokenModel>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Login)
                .LoginLength();

            RuleFor(x => x.Password)
                .PasswordLength();
        }
    }

    public class RequestHandler : IRequestHandler<Request, TokenModel>
    {
        private readonly AppDbContext _db;
        private readonly JwtGenerator _jwtGenerator;
        private readonly PasswordHasher _passwordHasher;

        public RequestHandler(
            AppDbContext db,
            JwtGenerator jwtGenerator,
            PasswordHasher passwordHasher)
        {
            _db = db;
            _jwtGenerator = jwtGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<TokenModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(x => x.Login == request.Login.ToLower(),
                cancellationToken);

            if (account is null)
            {
                throw new UnauthorizedException();
            }

            if (!_passwordHasher.Verify(request.Password, account.PasswordHash))
            {
                throw new UnauthorizedException();
            }

            var jwt = _jwtGenerator.Generate(account);

            return new(jwt);
        }
    }
}
