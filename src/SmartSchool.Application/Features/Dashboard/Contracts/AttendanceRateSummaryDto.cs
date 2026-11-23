namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class AttendanceRateSummaryDto
{
    /// <summary>
    /// Tingkat kehadiran hari ini dalam persen
    /// ((Hadir + Terlambat) / Total siswa aktif).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Jumlah siswa yang hadir hari ini (Hadir + Terlambat).
    /// </summary>
    public int AttendedCount { get; set; }

    /// <summary>
    /// Selisih tingkat kehadiran dibanding kemarin (poin persen).
    /// Null jika kemarin tidak ada data presensi.
    /// </summary>
    public decimal? ChangeFromYesterday { get; set; }
}
