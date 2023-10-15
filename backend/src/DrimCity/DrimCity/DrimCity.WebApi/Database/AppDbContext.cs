using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Database;

public class AppDbContext : DbContext
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Account> Accounts { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapAccount(modelBuilder);
        MapPost(modelBuilder);
    }

    private static void MapAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(x => x.Id);

            account.Property(x => x.Id)
                .UseIdentityAlwaysColumn();

            account.Property(x => x.Login)
                .IsRequired()
                .HasMaxLength(Account.LoginMaxLength);

            account.Property(x => x.PasswordHash)
                .IsRequired();

            account.Property(x => x.CreatedAt)
                .IsRequired();

            account.HasIndex(x => x.Login)
                .IsUnique();
        });
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
