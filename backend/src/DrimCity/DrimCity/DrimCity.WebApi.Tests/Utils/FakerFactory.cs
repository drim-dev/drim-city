using AutoBogus;
using DrimCity.WebApi.Domain;

namespace DrimCity.WebApi.Tests.Utils;

public static class FakerFactory
{
    public static Account CreateAccount(string? login = null, string? passwordHash = null) =>
        new AutoFaker<Account>()
            .RuleFor(a => a.Id, 0)
            .RuleFor(a => a.Login, f => login?.ToLower() ?? f.Random.AlphaNumeric(12))
            .RuleFor(a => a.CreatedAt, DateTime.UtcNow)
            .RuleFor(a => a.PasswordHash, f => passwordHash ?? f.Random.AlphaNumeric(12))
            .Generate();

    public static Post CreatePost(int authorId) =>
        new AutoFaker<Post>()
            .RuleFor(p => p.Id, 0)
            .RuleFor(p => p.CreatedAt, DateTime.UtcNow)
            .RuleFor(p => p.AuthorId, authorId)
            .Ignore(p => p.Author)
            .RuleFor(p => p.Slug, f => f.Random.AlphaNumeric(16))
            .Generate();

    public static Comment CreateComment(int authorId, int postId, DateTime? createdAt) =>
        new AutoFaker<Comment>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.AuthorId, authorId)
            .Ignore(x => x.Author)
            .RuleFor(x => x.PostId, postId)
            .Ignore(x => x.Post)
            .RuleFor(x => x.CreatedAt, createdAt ?? DateTime.UtcNow)
            .Generate();
}
