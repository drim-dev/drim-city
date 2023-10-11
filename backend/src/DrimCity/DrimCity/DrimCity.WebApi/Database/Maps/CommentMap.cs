using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrimCity.WebApi.Database.Maps;

public static class CommentMap
{
    public static void Build(EntityTypeBuilder<Comment> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(Comment.ContentMaxLength);

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.Property(x => x.AuthorId)
            .IsRequired();

        entity.Property(x => x.PostId)
            .IsRequired();

        entity
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(comment => comment.PostId);
    }
}