using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.ConfigureBaseEntity();

        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(256);
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(45);
        builder.Property(rt => rt.RevokedByIp).HasMaxLength(45);

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.UserId);

        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsActive);
    }
}
