using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Database;

public class AppDbContext : DbContext
{
    public DbSet<Post> Posts { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapPost(modelBuilder);
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
}
