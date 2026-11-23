namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class ViolationSummaryDto
{
    /// <summary>
    /// Jumlah pelanggaran pada bulan berjalan.
    /// </summary>
    public int ThisMonth { get; set; }

    /// <summary>
    /// Jumlah pelanggaran pada bulan sebelumnya.
    /// </summary>
    public int LastMonth { get; set; }

    /// <summary>
    /// ThisMonth - LastMonth (negatif berarti berkurang).
    /// </summary>
    public int ChangeFromLastMonth { get; set; }
}
