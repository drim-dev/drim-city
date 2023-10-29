namespace DrimCity.WebApi.Tests.Features.Posts.Contracts;

public record CommentContract(int Id, string Content, DateTime CreatedAt, int AuthorId);