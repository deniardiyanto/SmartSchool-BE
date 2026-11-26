using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
using SmartSchool.Application.Features.Reports.Contracts;
using SmartSchool.Application.Features.Reports.Interfaces;

namespace SmartSchool.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// Rekap presensi per siswa pada periode.
    /// </summary>
    [HttpGet("attendance")]
    [ProducesResponseType(
        typeof(ApiResponse<AttendanceReportResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Attendance(
        [FromQuery] ReportFilterRequest request)
    {
        var result = await _service.GetAttendanceReportAsync(request);

        return Ok(ApiResponse<AttendanceReportResponse>.Ok(
            result,
            "Attendance report loaded."));
    }

    /// <summary>
    /// Rekap poin pelanggaran per siswa pada periode.
    /// </summary>
    [HttpGet("violations")]
    [ProducesResponseType(
        typeof(ApiResponse<ViolationReportResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Violations(
        [FromQuery] ReportFilterRequest request)
    {
        var result = await _service.GetViolationReportAsync(request);

        return Ok(ApiResponse<ViolationReportResponse>.Ok(
            result,
            "Violation report loaded."));
    }

    /// <summary>
    /// Rekap KAIH per siswa pada periode.
    /// </summary>
    [HttpGet("kaih")]
    [ProducesResponseType(
        typeof(ApiResponse<KaihReportResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Kaih(
        [FromQuery] ReportFilterRequest request)
    {
        var result = await _service.GetKaihReportAsync(request);

        return Ok(ApiResponse<KaihReportResponse>.Ok(
            result,
            "KAIH report loaded."));
    }
}
