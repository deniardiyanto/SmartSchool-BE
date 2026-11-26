namespace SmartSchool.Application.Features.Reports.Contracts;

public class AttendanceReportResponse
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    /// <summary>
    /// Jumlah tanggal yang punya data presensi pada periode (hari efektif).
    /// </summary>
    public int EffectiveDays { get; set; }

    public List<AttendanceReportItemDto> Items { get; set; } = new();
}

public class AttendanceReportItemDto
{
    public Guid StudentId { get; set; }

    public string NIS { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string ClassRoomName { get; set; } = string.Empty;

    public int Present { get; set; }

    public int Late { get; set; }

    public int Sick { get; set; }

    public int Permission { get; set; }

    public int Absent { get; set; }

    /// <summary>
    /// (Hadir + Terlambat) / hari efektif, dalam persen.
    /// </summary>
    public decimal AttendanceRate { get; set; }
}
