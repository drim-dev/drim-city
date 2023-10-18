using DrimCity.WebApi.Database.Maps;
using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Post>(PostMap.Build)
            .Entity<Account>(AccountMap.Build)
            .Entity<Comment>(CommentMap.Build)
            ;
    }
}