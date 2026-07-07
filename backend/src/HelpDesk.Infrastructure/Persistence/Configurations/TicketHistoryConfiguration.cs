using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.ToTable("TicketHistories");
        builder.ConfigureBaseEntity();

        builder.Property(h => h.FieldName).IsRequired().HasMaxLength(100);
        builder.Property(h => h.OldValue).HasMaxLength(1000);
        builder.Property(h => h.NewValue).HasMaxLength(1000);

        builder.HasOne(h => h.Ticket)
            .WithMany(t => t.HistoryEntries)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany(u => u.HistoryEntries)
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(h => h.TicketId);
    }
}
