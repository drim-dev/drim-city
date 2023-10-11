using DrimCity.WebApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrimCity.WebApi.Database.Maps;

public static class AccountMap
{
    public static void Build(EntityTypeBuilder<Account> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.Login)
            .IsRequired()
            .HasMaxLength(Account.LoginMaxLength);

        entity.Property(x => x.PasswordHash)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.HasIndex(x => x.Login)
            .IsUnique();
    }
}