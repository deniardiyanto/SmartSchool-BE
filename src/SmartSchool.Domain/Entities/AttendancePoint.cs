using SmartSchool.Domain.Common;

namespace SmartSchool.Domain.Entities;

public class AttendancePoint : BaseAuditableEntity
{
    public Guid StudentId { get; set; }

    public Guid AttendanceId { get; set; }

    /// <summary>
    /// Poin pelanggaran. Positif menambah poin (contoh: 5 untuk terlambat),
    /// negatif mengurangi (koreksi/pembinaan).
    /// </summary>
    public int Point { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime PointDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public Student Student { get; set; } = null!;

    public Attendance Attendance { get; set; } = null!;
     public string? Description { get; set; }
}