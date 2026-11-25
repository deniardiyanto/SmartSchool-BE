using SmartSchool.Application.Features.Settings.Contracts;

namespace SmartSchool.Application.Features.Settings.Interfaces;

public interface ISettingsService
{
    /// <summary>
    /// Aturan presensi aktif, atau null bila belum dikonfigurasi.
    /// </summary>
    Task<AttendanceRuleDto?> GetAttendanceRuleAsync();

    Task<AttendanceRuleDto> SaveAttendanceRuleAsync(
        SaveAttendanceRuleRequest request);

    Task<MonitoringSettingDto> GetMonitoringSettingAsync();

    Task<MonitoringSettingDto> SaveMonitoringSettingAsync(
        SaveMonitoringSettingRequest request);
}
