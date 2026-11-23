namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class PresenceRecapItemDto
{
    /// <summary>
    /// present | sick | permission | absent | late
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Hadir | Sakit | Izin | Alpa | Terlambat
    /// </summary>
    public string Label { get; set; } = string.Empty;

    public int Count { get; set; }

    /// <summary>
    /// Persentase terhadap total siswa aktif (1 desimal).
    /// </summary>
    public decimal Percentage { get; set; }
}
