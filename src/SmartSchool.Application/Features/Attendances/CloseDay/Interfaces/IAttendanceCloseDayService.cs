using SmartSchool.Application.Features.Attendances.CloseDay.Contracts;

namespace SmartSchool.Application.Features.Attendances.CloseDay.Interfaces;

public interface IAttendanceCloseDayService
{
    /// <summary>
    /// Menandai Alpa semua siswa aktif yang belum punya presensi pada
    /// tanggal tersebut, beserta poin Alpa dari aturan presensi.
    /// Aman dijalankan berulang (siswa yang sudah punya presensi dilewati).
    /// </summary>
    Task<CloseDayResponse> CloseDayAsync(CloseDayRequest request);
}
