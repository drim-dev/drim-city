using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Hosting;
using System.Data;

namespace DrimCity.WebApi.Database.EntitiesBuilder.Features.Posts;

public class PostEntitiesBuilder: IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(post => post.Id);
        builder.Property(post => post.Id).HasColumnName("Id").UseIdentityAlwaysColumn();
        builder.Property(post => post.Title).IsRequired().HasMaxLength(Post.TitleMaxLength);
        builder.Property(post => post.Content).IsRequired().HasMaxLength(Post.ContentMaxLength);
        builder.Property(post => post.CreatedAt).IsRequired();
        builder.Property(post => post.AuthorId).IsRequired();//TODO relations with Entity<Author> one<Author> to many<Post>
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(Post.SlugMaxLength);
        builder.HasIndex(post => post.Slug).IsUnique();
    }
}
