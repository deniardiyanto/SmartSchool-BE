namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class PresenceRecapDto
{
    /// <summary>
    /// Total siswa aktif.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Siswa aktif yang belum punya data presensi hari ini.
    /// Sudah termasuk dalam hitungan item "absent" (Alpa).
    /// </summary>
    public int NotRecorded { get; set; }

    /// <summary>
    /// Urutan: Hadir, Sakit, Izin, Alpa, Terlambat.
    /// </summary>
    public List<PresenceRecapItemDto> Items { get; set; } = new();
}
