using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class TicketAssignmentConfiguration : IEntityTypeConfiguration<TicketAssignment>
{
    public void Configure(EntityTypeBuilder<TicketAssignment> builder)
    {
        builder.ToTable("TicketAssignments");
        builder.ConfigureBaseEntity();

        builder.Property(a => a.AssignmentType).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PreviousAssignedToUser)
            .WithMany()
            .HasForeignKey(a => a.PreviousAssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(a => a.AssignedToUser)
            .WithMany()
            .HasForeignKey(a => a.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(a => a.AssignedByUser)
            .WithMany()
            .HasForeignKey(a => a.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(a => a.TicketId);
        builder.HasIndex(a => a.AssignedToUserId);
    }
}
