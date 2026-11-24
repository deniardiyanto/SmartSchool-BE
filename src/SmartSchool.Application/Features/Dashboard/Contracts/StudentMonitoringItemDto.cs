namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class StudentMonitoringItemDto
{
    public Guid StudentId { get; set; }

    public string NIS { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public Guid ClassRoomId { get; set; }

    public string ClassRoomName { get; set; } = string.Empty;

    /// <summary>
    /// Presensi bulan berjalan dalam persen
    /// (hari Hadir + Terlambat / hari efektif bulan ini).
    /// </summary>
    public decimal AttendanceRate { get; set; }

    /// <summary>
    /// Akumulasi poin pelanggaran (semakin tinggi semakin buruk).
    /// </summary>
    public int ViolationPoints { get; set; }

    /// <summary>
    /// Rata-rata kebiasaan KAIH yang terpenuhi bulan berjalan
    /// (dari hari yang diisi), dalam persen.
    /// </summary>
    public decimal KaihRate { get; set; }

    /// <summary>
    /// Jumlah hari KAIH diisi bulan berjalan.
    /// 0 berarti belum pernah mengisi (KaihRate 0 bukan "semua tidak terpenuhi").
    /// </summary>
    public int KaihSubmittedDays { get; set; }

    /// <summary>
    /// active | warning | coaching
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Aktif | Peringatan | Pembinaan
    /// </summary>
    public string StatusLabel { get; set; } = string.Empty;
}
