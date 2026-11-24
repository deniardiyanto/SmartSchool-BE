using SmartSchool.Domain.Common;

namespace SmartSchool.Domain.Entities;

/// <summary>
/// Catatan aktivitas yang tampil di "Aktivitas Terbaru" dashboard.
/// Bersifat append-only, jadi tidak memakai soft delete.
/// </summary>
public class ActivityLog : BaseEntity
{
    /// <summary>
    /// Contoh: student_created, attendance_updated, violation_added
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Id data terkait (siswa, presensi, poin, dst).
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Pelaku. Null jika tidak diketahui (belum login / aksi sistem).
    /// </summary>
    public Guid? ActorUserId { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? ActorUser { get; set; }
}
