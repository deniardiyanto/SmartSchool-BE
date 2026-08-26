using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartSchool.Domain.Entities;

namespace SmartSchool.Infrastructure.Persistence.Configurations;

public class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.ToTable("guardians");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(255);

        builder.Property(x => x.Occupation)
            .HasMaxLength(100);

        builder.Property(x => x.Relationship)
            .HasConversion<int>();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasMany(x => x.Students)
            .WithOne(x => x.Guardian)
            .HasForeignKey(x => x.GuardianId)
            .OnDelete(DeleteBehavior.Restrict);

     builder.Property(x => x.GuardianCode)
    .HasMaxLength(30)
    .IsRequired();

   builder.HasIndex(x => x.GuardianCode)
    .IsUnique()
    .HasFilter("\"IsDeleted\" = false")
    .HasDatabaseName("IX_guardians_GuardianCode");
    }
}