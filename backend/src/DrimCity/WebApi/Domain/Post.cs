namespace WebApi.Domain;

public class Post
{
    public const int TitleMaxLength = 300;
    public const int ContentMaxLength = 100_000;

    public Post(uint id, string title, string content, DateTime createdAt, uint authorId, string slug)
    {
        Id = id;
        Title = title;
        Content = content;
        CreatedAt = createdAt;
        AuthorId = authorId;
        Slug = slug;
    }

    public uint Id { get; private set; }

    public string Title { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public uint AuthorId { get; private set; }

    public string Slug { get; private set; }
}
