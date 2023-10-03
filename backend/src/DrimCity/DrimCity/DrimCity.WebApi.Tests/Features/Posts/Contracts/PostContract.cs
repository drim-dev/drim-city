namespace DrimCity.WebApi.Tests.Features.Posts.Contracts;

public record PostContract(int Id, string Title, string Content, DateTime CreatedAt, int AuthorId, string Slug);
