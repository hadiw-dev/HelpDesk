using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Description).HasMaxLength(500);

        builder.HasData(
            Seeded(SeedIds.Roles.Admin, "Admin", "Full system access, including user and configuration management."),
            Seeded(SeedIds.Roles.ItSupportAgent, "IT Support Agent", "Handles, triages and resolves support tickets."),
            Seeded(SeedIds.Roles.Employee, "Employee", "Submits and tracks their own support tickets."),
            Seeded(SeedIds.Roles.Manager, "Manager", "Views team tickets and reporting dashboards.")
        );
    }

    private static ApplicationRole Seeded(Guid id, string name, string description) => new()
    {
        Id = id,
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        Description = description,
        ConcurrencyStamp = id.ToString()
    };
}
