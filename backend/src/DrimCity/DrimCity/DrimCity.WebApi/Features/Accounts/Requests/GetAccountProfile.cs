using Common.Web.Endpoints;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Auth.Validation;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Features.Accounts.Requests;

public static class GetAccountProfile
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapGet("/accounts/{login}", async Task<Results<Ok<Response>, NotFound>>
                (IMediator mediator, string login, CancellationToken cancellationToken) =>
            {
                var account = await mediator.Send(new Request(login), cancellationToken);

                if (account is null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(account);
            });
        }
    }

    public record Request(string Login) : IRequest<Response?>;

    public record Response(string Login, DateTime CreatedAt);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Login)
                .LoginLength();
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response?>
    {
        private readonly AppDbContext _db;

        public RequestHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var account = await _db.Accounts
                .Where(x => x.Login == request.Login.ToLower())
                .SingleOrDefaultAsync(cancellationToken);

            return account is null
                ? null
                : new Response(account.Login, account.CreatedAt);
        }
    }
}
