using AutoBogus;
using DrimCity.WebApi.Domain;

namespace DrimCity.WebApi.Tests.Utils;

public static class FakerFactory
{
    public static Account CreateAccount(string? login = null, string? passwordHash = null) =>
        new AutoFaker<Account>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.Login, x => login?.ToLower() ?? x.Random.AlphaNumeric(12))
            .RuleFor(x => x.CreatedAt, DateTime.UtcNow)
            .RuleFor(x => x.PasswordHash, x => passwordHash ?? x.Random.AlphaNumeric(12))
            .Generate();
}
