namespace DrimCity.WebApi.Features.Posts.Models;

public record PostModel(int Id, string Title, string Content, DateTime CreatedAt, int AuthorId, string Slug);
