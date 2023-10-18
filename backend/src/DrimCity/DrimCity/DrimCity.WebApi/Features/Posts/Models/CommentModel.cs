namespace DrimCity.WebApi.Features.Posts.Models;

public record CommentModel(int Id, string Content, DateTime CreatedAt, int AuthorId);