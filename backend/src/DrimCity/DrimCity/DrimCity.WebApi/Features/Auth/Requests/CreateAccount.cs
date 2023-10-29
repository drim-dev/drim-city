using Common.Web.Endpoints;
using Common.Web.Errors.Exceptions;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Auth.Errors;
using DrimCity.WebApi.Features.Auth.Models;
using DrimCity.WebApi.Features.Auth.Services;
using DrimCity.WebApi.Features.Auth.Validation;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static DrimCity.WebApi.Features.Auth.Errors.AuthValidationErrors;

namespace DrimCity.WebApi.Features.Auth.Requests;

public static class CreateAccount
{
    public class Endpoint : IEndpoint
    {
        private const string Path = "/auth/accounts";

        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Created<AccountModel>>
                (IMediator mediator, Request request, CancellationToken cancellationToken) =>
            {
                var account = await mediator.Send(request, cancellationToken);
                return TypedResults.Created($"{Path}/{account.Login}", account);
            });
        }
    }

    public record Request(string Login, string Password) : IRequest<AccountModel>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Login)
                .LoginLength()
                .Matches(@"^[a-zA-Z0-9_\-]*$")
                    .WithMessage("Login must contain only letters, numbers, underscores and dashes")
                    .WithErrorCode(LoginMustContainSpecificSymbols);

            RuleFor(x => x.Password)
                .PasswordLength()
                .Matches("[A-Z]+")
                    .WithMessage("Password must contain at least one uppercase letter")
                    .WithErrorCode(PasswordMustContainUppercaseLetter)
                .Matches("[a-z]+")
                    .WithMessage("Password must contain at least one lowercase letter")
                    .WithErrorCode(PasswordMustContainLowercaseLetter)
                .Matches("[0-9]+")
                    .WithMessage("Password must contain at least one number")
                    .WithErrorCode(PasswordMustContainNumber)
                .Matches(@"[\!\?\*\.\+]+")
                    .WithMessage("Your password must contain at least one of the symbols in (!?*.+).")
                    .WithErrorCode(PasswordMustContainSpecialSymbol);
        }
    }

    public class RequestHandler : IRequestHandler<Request, AccountModel>
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher _passwordHasher;

        public RequestHandler(
            AppDbContext db,
            PasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        public async Task<AccountModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var existingAccount = await _db.Accounts.SingleOrDefaultAsync(x => x.Login == request.Login.ToLower(),
                cancellationToken);

            if (existingAccount is not null)
            {
                throw new LogicConflictException("Account already exists",
                    AuthLogicConflictErrors.AccountAlreadyExists);
            }

            var passwordHash = _passwordHasher.Hash(request.Password);

            var account = new Account(0, request.Login.ToLower(), passwordHash, DateTime.UtcNow);

            await _db.Accounts.AddAsync(account, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new(account.Login, account.CreatedAt);
        }
    }
}
