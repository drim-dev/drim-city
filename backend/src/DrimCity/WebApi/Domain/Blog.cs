namespace WebApi.Domain;

public class Blog
{
    public const int TitleMaxLength = 300;
    public const int ContentMaxLength = 100_000;
    public const int SlugMaxLength = 400;

    public Blog(int id, string title, string content, DateTime createdAt, int authorId, string slug)
    {
        Id = id;
        Title = title;
        Content = content;
        CreatedAt = createdAt;
        UpdatedAt = DateTime.Now;
        AuthorId = authorId;
        Slug = slug;
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public int AuthorId { get; private set; }

    public string Slug { get; private set; }
}