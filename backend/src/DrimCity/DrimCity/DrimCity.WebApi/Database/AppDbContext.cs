using DrimCity.WebApi.Database.Maps;
using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Post>(PostMap.Build)
            .Entity<Account>(AccountMap.Build)
            .Entity<Comment>(CommentMap.Build)
            ;
    }
}