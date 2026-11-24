namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class BestClassDto
{
    /// <summary>
    /// Peringkat 1..N (1 = kehadiran tertinggi).
    /// </summary>
    public int Rank { get; set; }

    public Guid ClassRoomId { get; set; }

    public string ClassRoomName { get; set; } = string.Empty;

    /// <summary>
    /// Tingkat kehadiran hari ini dalam persen
    /// ((Hadir + Terlambat) / siswa aktif di kelas).
    /// </summary>
    public decimal AttendanceRate { get; set; }

    public int AttendedCount { get; set; }

    public int TotalStudents { get; set; }
}
