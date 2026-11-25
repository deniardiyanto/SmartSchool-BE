using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartSchool.Domain.Entities;

namespace SmartSchool.Infrastructure.Persistence.Configurations;

public class MonitoringSettingConfiguration
    : IEntityTypeConfiguration<MonitoringSetting>
{
    public void Configure(EntityTypeBuilder<MonitoringSetting> builder)
    {
        builder.ToTable("monitoring_settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);
    }
}
