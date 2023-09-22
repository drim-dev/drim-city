using DrimCity.WebApi.Database.EntitiesBuilder.Features.Posts;
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
        modelBuilder.ApplyConfiguration(new PostEntitiesBuilder());
    }
}
