namespace SmartSchool.Application.Features.Attendances.CloseDay.Contracts;

public class CloseDayRequest
{
    /// <summary>
    /// Tanggal presensi (format yyyy-MM-dd, sama seperti tanggal presensi
    /// lain). Kosong = hari ini.
    /// </summary>
    public DateOnly? Date { get; set; }
}
