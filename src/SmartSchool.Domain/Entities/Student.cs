using SmartSchool.Domain.Common;
using SmartSchool.Domain.Enums;

namespace SmartSchool.Domain.Entities;

public class Student : BaseAuditableEntity
{
    public string NIS { get; set; } = string.Empty;

    public string? NISN { get; set; }

    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public string? BirthPlace { get; set; }

    public DateTime BirthDate { get; set; }

    public string? Address { get; set; }

    public string? PhotoUrl { get; set; }

    public Guid ClassRoomId { get; set; }

    public Guid GuardianId { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ClassRoom ClassRoom { get; set; } = null!;

    public Guardian Guardian { get; set; } = null!;

    public BarcodeCard? BarcodeCard { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<AttendancePoint> AttendancePoints { get; set; } = new List<AttendancePoint>();

    public ICollection<KaihEntry> KaihEntries { get; set; } = new List<KaihEntry>();
}