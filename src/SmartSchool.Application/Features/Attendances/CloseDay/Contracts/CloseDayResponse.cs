namespace SmartSchool.Application.Features.Attendances.CloseDay.Contracts;

public class CloseDayResponse
{
    public DateOnly Date { get; set; }

    /// <summary>
    /// Jumlah siswa yang baru ditandai Alpa pada proses ini.
    /// </summary>
    public int MarkedAbsent { get; set; }

    /// <summary>
    /// Jumlah siswa aktif yang sudah punya presensi (dilewati).
    /// </summary>
    public int AlreadyRecorded { get; set; }

    public int TotalActiveStudents { get; set; }

    /// <summary>
    /// Poin pelanggaran per siswa Alpa (dari aturan presensi).
    /// </summary>
    public int AbsentPoint { get; set; }
}
