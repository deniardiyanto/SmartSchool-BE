namespace SmartSchool.Application.Features.Reports.Contracts;

public class ReportFilterRequest
{
    /// <summary>
    /// Awal periode (yyyy-MM-dd). Kosong = tanggal 1 bulan ini.
    /// </summary>
    public DateOnly? From { get; set; }

    /// <summary>
    /// Akhir periode, termasuk (yyyy-MM-dd). Kosong = hari ini.
    /// </summary>
    public DateOnly? To { get; set; }

    public Guid? ClassRoomId { get; set; }
}
