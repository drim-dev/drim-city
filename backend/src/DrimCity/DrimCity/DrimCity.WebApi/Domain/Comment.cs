namespace DrimCity.WebApi.Domain;

public class Comment
{
    public const int ContentMaxLength = 10_000;

    public Comment(string content, DateTime createdAt, int authorId, int postId)
    {
        Id = 0;
        Content = content;
        CreatedAt = createdAt;
        AuthorId = authorId;
        PostId = postId;
    }

    public int Id { get; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int AuthorId { get; private set; }
    public Account? Author { get; private set; }
    public int PostId { get; private set; }
    public Post? Post { get; private set; }
}
