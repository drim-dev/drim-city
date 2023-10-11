using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrimCity.WebApi.Database.Maps;

public static class PostMap
{
    public static void Build(EntityTypeBuilder<Post> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(Post.TitleMaxLength);

        entity.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(Post.ContentMaxLength);

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.Property(x => x.AuthorId)
            .IsRequired();

        entity.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(Post.SlugMaxLength);

        entity.HasIndex(x => x.Slug)
            .IsUnique();
    }
}