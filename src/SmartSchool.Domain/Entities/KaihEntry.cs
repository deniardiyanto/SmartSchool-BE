using SmartSchool.Domain.Common;

namespace SmartSchool.Domain.Entities;

/// <summary>
/// Isian harian 7 Kebiasaan Anak Indonesia Hebat (KAIH) per siswa.
/// Satu baris = satu siswa pada satu tanggal.
/// </summary>
public class KaihEntry : BaseAuditableEntity
{
    public Guid StudentId { get; set; }

    public DateOnly EntryDate { get; set; }

    /// <summary>
    /// Bangun pagi
    /// </summary>
    public bool WakeUpEarly { get; set; }

    /// <summary>
    /// Beribadah
    /// </summary>
    public bool Worship { get; set; }

    /// <summary>
    /// Berolahraga
    /// </summary>
    public bool Exercise { get; set; }

    /// <summary>
    /// Makan sehat dan bergizi
    /// </summary>
    public bool HealthyEating { get; set; }

    /// <summary>
    /// Gemar belajar
    /// </summary>
    public bool Learning { get; set; }

    /// <summary>
    /// Bermasyarakat
    /// </summary>
    public bool Socializing { get; set; }

    /// <summary>
    /// Tidur cepat
    /// </summary>
    public bool EarlySleep { get; set; }

    // Navigation
    public Student Student { get; set; } = null!;
}
