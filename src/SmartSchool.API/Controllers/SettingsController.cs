using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
using SmartSchool.Application.Features.Settings.Contracts;
using SmartSchool.Application.Features.Settings.Interfaces;

namespace SmartSchool.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _service;

    public SettingsController(ISettingsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Aturan presensi aktif (data null bila belum dikonfigurasi).
    /// </summary>
    [HttpGet("attendance-rule")]
    [ProducesResponseType(
        typeof(ApiResponse<AttendanceRuleDto?>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceRule()
    {
        var result = await _service.GetAttendanceRuleAsync();

        return Ok(ApiResponse<AttendanceRuleDto?>.Ok(
            result,
            "Attendance rule loaded."));
    }

    /// <summary>
    /// Simpan aturan presensi aktif (dibuat bila belum ada).
    /// </summary>
    [HttpPut("attendance-rule")]
    [ProducesResponseType(
        typeof(ApiResponse<AttendanceRuleDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveAttendanceRule(
        [FromBody] SaveAttendanceRuleRequest request)
    {
        var result = await _service.SaveAttendanceRuleAsync(request);

        return Ok(ApiResponse<AttendanceRuleDto>.Ok(
            result,
            "Attendance rule saved."));
    }

    /// <summary>
    /// Ambang status siswa (Peringatan / Pembinaan).
    /// </summary>
    [HttpGet("student-monitoring")]
    [ProducesResponseType(
        typeof(ApiResponse<MonitoringSettingDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonitoringSetting()
    {
        var result = await _service.GetMonitoringSettingAsync();

        return Ok(ApiResponse<MonitoringSettingDto>.Ok(
            result,
            "Monitoring setting loaded."));
    }

    [HttpPut("student-monitoring")]
    [ProducesResponseType(
        typeof(ApiResponse<MonitoringSettingDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveMonitoringSetting(
        [FromBody] SaveMonitoringSettingRequest request)
    {
        var result = await _service.SaveMonitoringSettingAsync(request);

        return Ok(ApiResponse<MonitoringSettingDto>.Ok(
            result,
            "Monitoring setting saved."));
    }
}
