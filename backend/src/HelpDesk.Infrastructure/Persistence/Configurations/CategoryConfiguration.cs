using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            Seeded(SeedIds.Categories.Hardware, "Hardware", "Physical devices such as desktops, laptops, printers and peripherals.", 1),
            Seeded(SeedIds.Categories.Software, "Software", "Application installation, licensing and configuration issues.", 2),
            Seeded(SeedIds.Categories.Network, "Network", "Connectivity, VPN and network infrastructure issues.", 3),
            Seeded(SeedIds.Categories.Email, "Email", "Mailbox, delivery and email client issues.", 4),
            Seeded(SeedIds.Categories.AccessRequest, "Access Request", "Requests for new or changed system/application access.", 5),
            Seeded(SeedIds.Categories.Other, "Other", "Requests that do not fit another category.", 6)
        );
    }

    private static Category Seeded(Guid id, string name, string description, int displayOrder) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        DisplayOrder = displayOrder,
        IsActive = true,
        CreatedAt = SeedIds.SeedDate
    };
}
