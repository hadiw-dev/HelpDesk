using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.ConfigureBaseEntity();

        builder.Property(s => s.SiteName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.AllowedFileExtensions).IsRequired().HasMaxLength(500);

        builder.HasData(new SystemSetting
        {
            Id = SeedIds.SystemSettings.Singleton,
            SiteName = "HelpDesk System",
            MaxFileUploadSizeMb = 10,
            AllowedFileExtensions = ".pdf,.png,.jpg,.jpeg,.docx,.xlsx,.txt,.zip",
            DefaultPageSize = 20,
            CreatedAt = SeedIds.SeedDate,
        });
    }
}
