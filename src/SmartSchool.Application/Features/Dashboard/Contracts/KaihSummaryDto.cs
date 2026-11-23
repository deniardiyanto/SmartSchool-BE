namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class KaihSummaryDto
{
    /// <summary>
    /// Persentase siswa aktif yang sudah mengisi KAIH hari ini
    /// (KAIH Terkumpul).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Jumlah siswa aktif yang sudah mengisi KAIH hari ini.
    /// </summary>
    public int SubmittedCount { get; set; }

    /// <summary>
    /// Rata-rata kebiasaan yang terpenuhi dari 7 kebiasaan
    /// pada isian hari ini, dalam persen. 0 jika belum ada isian.
    /// </summary>
    public decimal AverageHabitCompletion { get; set; }

    /// <summary>
    /// Selisih Rate dibanding hari yang sama minggu lalu (poin persen).
    /// Null jika minggu lalu tidak ada isian.
    /// </summary>
    public decimal? ChangeFromLastWeek { get; set; }
}
