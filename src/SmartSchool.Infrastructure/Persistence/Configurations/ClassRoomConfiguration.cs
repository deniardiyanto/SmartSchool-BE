using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartSchool.Domain.Entities;

namespace SmartSchool.Infrastructure.Persistence.Configurations;

public class ClassRoomConfiguration : IEntityTypeConfiguration<ClassRoom>
{
    public void Configure(EntityTypeBuilder<ClassRoom> builder)
    {
        builder.ToTable("class_rooms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        // builder.HasIndex(x => x.Code)
        //     .IsUnique();
        builder.HasIndex(x => new
        {
            x.Code,
            x.AcademicYear
        })
.IsUnique()
.HasFilter("\"IsDeleted\" = false");

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Grade)
            .IsRequired();

        builder.Property(x => x.AcademicYear)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasMany(x => x.Students)
            .WithOne(s => s.ClassRoom)
            .HasForeignKey(s => s.ClassRoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}