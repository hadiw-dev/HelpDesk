using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
{
    public void Configure(EntityTypeBuilder<Priority> builder)
    {
        builder.ToTable("Priorities");
        builder.ConfigureBaseEntity();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.HasIndex(p => p.Name).IsUnique();

        builder.HasData(
            Seeded(SeedIds.Priorities.Low, "Low", "Minor issue with no significant impact on work.", 1),
            Seeded(SeedIds.Priorities.Medium, "Medium", "Moderate impact; should be addressed in normal course of business.", 2),
            Seeded(SeedIds.Priorities.High, "High", "Significant impact on productivity; needs prompt attention.", 3),
            Seeded(SeedIds.Priorities.Critical, "Critical", "Severe impact; business operations are blocked.", 4)
        );
    }

    private static Priority Seeded(Guid id, string name, string description, int displayOrder) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        DisplayOrder = displayOrder,
        IsActive = true,
        CreatedAt = SeedIds.SeedDate
    };
}
