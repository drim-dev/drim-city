namespace WebApi.Features.Posts.Models;

public record PostModel(uint Id, string Title, string Content, DateTime CreatedAt, uint AuthorId, string Slug);
