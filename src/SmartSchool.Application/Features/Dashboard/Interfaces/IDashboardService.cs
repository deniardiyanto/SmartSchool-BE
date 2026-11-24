using SmartSchool.Application.Features.Dashboard.Contracts;

namespace SmartSchool.Application.Features.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<AttendanceDashboardResponse>
        GetAttendanceDashboardAsync();

    /// <summary>
    /// Ringkasan dashboard admin CMS (kartu statistik,
    /// rekap presensi hari ini, aktivitas terbaru).
    /// </summary>
    Task<DashboardOverviewResponse>
        GetOverviewAsync(int activityLimit);

    /// <summary>
    /// Kelas dengan tingkat kehadiran tertinggi hari ini.
    /// </summary>
    Task<List<BestClassDto>>
        GetBestClassesAsync(int top);

    /// <summary>
    /// Monitoring peserta didik (presensi, poin, KAIH, status)
    /// dengan paginasi.
    /// </summary>
    Task<PagedStudentMonitoringResponse>
        GetStudentMonitoringAsync(StudentMonitoringFilterRequest request);
}