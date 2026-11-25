using SmartSchool.Domain.Common;

namespace SmartSchool.Domain.Entities;

/// <summary>
/// Ambang status siswa pada monitoring peserta didik.
/// Hanya ada satu baris aktif; bila belum ada, dipakai nilai bawaan
/// dari StudentMonitoringConstant.
/// </summary>
public class MonitoringSetting : BaseAuditableEntity
{
    /// <summary>
    /// Poin pelanggaran minimal untuk status "Peringatan".
    /// </summary>
    public int WarningPoint { get; set; }

    /// <summary>
    /// Poin pelanggaran minimal untuk status "Pembinaan".
    /// </summary>
    public int CoachingPoint { get; set; }
}
