using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartSchool.Domain.Entities;

namespace SmartSchool.Infrastructure.Persistence.Configurations;

public class KaihEntryConfiguration : IEntityTypeConfiguration<KaihEntry>
{
    public void Configure(EntityTypeBuilder<KaihEntry> builder)
    {
        builder.ToTable("kaih_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntryDate)
            .HasColumnType("date");

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.KaihEntries)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.StudentId,
            x.EntryDate
        }).IsUnique();
    }
}
