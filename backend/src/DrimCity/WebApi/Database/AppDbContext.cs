using Microsoft.EntityFrameworkCore;
using WebApi.Domain;

namespace WebApi.Database;

public class AppDbContext : DbContext
{
    public DbSet<Post> Posts { get; set; }
    
    public DbSet<Blog> Blogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapPost(modelBuilder);
        MapBlog(modelBuilder);
    }

    private static void MapPost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(post =>
        {
            post.HasKey(x => x.Id);

            post.Property(x => x.Id)
                .UseIdentityAlwaysColumn();

            post.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(Post.TitleMaxLength);

            post.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(Post.ContentMaxLength);

            post.Property(x => x.CreatedAt)
                .IsRequired();

            post.Property(x => x.AuthorId)
                .IsRequired();

            post.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(Post.SlugMaxLength);

            post.HasIndex(x => x.Slug)
                .IsUnique();
        });
    }
    
    private static void MapBlog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(blog =>
        {
            blog.HasKey(x => x.Id);

            blog.Property(x => x.Id)
                .UseIdentityAlwaysColumn();

            blog.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(Post.TitleMaxLength);

            blog.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(Post.ContentMaxLength);

            blog.Property(x => x.CreatedAt)
                .IsRequired();
            
            blog.Property(x => x.UpdatedAt)
                .IsRequired();

            blog.Property(x => x.AuthorId)
                .IsRequired();

            blog.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(Post.SlugMaxLength);

            blog.HasIndex(x => x.Slug)
                .IsUnique();
        });
    }
}
