using AutoBogus;
using DrimCity.WebApi.Domain;

namespace DrimCity.WebApi.Tests.Features.Utils;

public static class FakerFactory
{
    public static Account CreateAccount(string login) =>
        new AutoFaker<Account>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.Login, login)
            .RuleFor(x => x.CreatedAt, DateTime.UtcNow)
            .RuleFor(x => x.PasswordHash, "1234567890")
            .Generate();
}
