using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SmartSchool.Domain.Entities;

namespace SmartSchool.Infrastructure.Persistence.Context;

public class SmartSchoolDbContext : DbContext
{
    public SmartSchoolDbContext(
        DbContextOptions<SmartSchoolDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<BarcodeCard> BarcodeCards => Set<BarcodeCard>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendancePoint> AttendancePoints => Set<AttendancePoint>();
    public DbSet<WhatsAppLog> WhatsAppLogs => Set<WhatsAppLog>();
    public DbSet<SchoolSetting> SchoolSettings => Set<SchoolSetting>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<AttendanceRule> AttendanceRules => Set<AttendanceRule>();
    public DbSet<KaihEntry> KaihEntries => Set<KaihEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartSchoolDbContext).Assembly);
    }
}