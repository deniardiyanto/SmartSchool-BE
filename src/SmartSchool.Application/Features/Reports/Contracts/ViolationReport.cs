namespace SmartSchool.Application.Features.Reports.Contracts;

public class ViolationReportResponse
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    /// <summary>
    /// Hanya siswa dengan poin pelanggaran > 0 pada periode.
    /// Urut dari poin tertinggi.
    /// </summary>
    public List<ViolationReportItemDto> Items { get; set; } = new();
}

public class ViolationReportItemDto
{
    public Guid StudentId { get; set; }

    public string NIS { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string ClassRoomName { get; set; } = string.Empty;

    /// <summary>
    /// Jumlah catatan poin positif (pelanggaran).
    /// </summary>
    public int Incidents { get; set; }

    /// <summary>
    /// Total poin periode (poin positif dikurangi koreksi, minimal 0).
    /// </summary>
    public int TotalPoint { get; set; }
}
