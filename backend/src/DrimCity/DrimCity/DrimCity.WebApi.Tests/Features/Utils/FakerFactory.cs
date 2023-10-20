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

    public static Post CreatePost(string slug) =>
        new AutoFaker<Post>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.Slug, slug)
            .RuleFor(x => x.CreatedAt, DateTime.UtcNow)
            .Generate();

    public static Comment CreateComment(int postId, DateTime? createdAt) =>
        new AutoFaker<Comment>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.PostId, postId)
            .RuleFor(x => x.Post, (Post)null!)
            .RuleFor(x => x.CreatedAt, createdAt ?? DateTime.UtcNow)
            .Generate();
}
