using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("Statuses");
        builder.ConfigureBaseEntity();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasData(
            Seeded(SeedIds.Statuses.Open, "Open", "Ticket has been submitted and is awaiting triage.", 1),
            Seeded(SeedIds.Statuses.InProgress, "In Progress", "An agent is actively working on the ticket.", 2),
            Seeded(SeedIds.Statuses.Pending, "Pending", "Waiting on additional information, typically from the requester.", 3),
            Seeded(SeedIds.Statuses.Resolved, "Resolved", "A fix has been applied and is awaiting confirmation.", 4),
            Seeded(SeedIds.Statuses.Closed, "Closed", "Ticket is fully closed and archived.", 5)
        );
    }

    private static Status Seeded(Guid id, string name, string description, int displayOrder) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        DisplayOrder = displayOrder,
        IsActive = true,
        CreatedAt = SeedIds.SeedDate
    };
}
