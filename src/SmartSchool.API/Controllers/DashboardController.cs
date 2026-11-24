using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
using SmartSchool.Application.Features.Dashboard.Contracts;
using SmartSchool.Application.Features.Dashboard.Interfaces;

namespace SmartSchool.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(
        IDashboardService service)
    {
        _service = service;
    }

    /// <summary>
    /// Ringkasan dashboard admin CMS.
    /// </summary>
    /// <param name="activityLimit">
    /// Jumlah aktivitas terbaru (default 5, maksimal 20).
    /// </param>
    [HttpGet("overview")]
    [ProducesResponseType(
        typeof(ApiResponse<DashboardOverviewResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(
        [FromQuery] int activityLimit = 5)
    {
        var result =
            await _service.GetOverviewAsync(activityLimit);

        return Ok(
            ApiResponse<DashboardOverviewResponse>.Ok(
                result,
                "Dashboard loaded."));
    }

    /// <summary>
    /// Kelas terbaik hari ini berdasarkan tingkat kehadiran.
    /// </summary>
    /// <param name="top">
    /// Jumlah kelas (default 3, maksimal 10).
    /// </param>
    [HttpGet("best-classes")]
    [ProducesResponseType(
        typeof(ApiResponse<List<BestClassDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBestClasses(
        [FromQuery] int top = 3)
    {
        var result =
            await _service.GetBestClassesAsync(top);

        return Ok(
            ApiResponse<List<BestClassDto>>.Ok(
                result,
                "Best classes loaded."));
    }

    /// <summary>
    /// Monitoring peserta didik.
    /// </summary>
    [HttpGet("student-monitoring")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedStudentMonitoringResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentMonitoring(
        [FromQuery] StudentMonitoringFilterRequest request)
    {
        var result =
            await _service.GetStudentMonitoringAsync(request);

        return Ok(
            ApiResponse<PagedStudentMonitoringResponse>.Ok(
                result,
                "Student monitoring loaded."));
    }

    /// <summary>
    /// Dashboard attendance hari ini
    /// </summary>
    [HttpGet("attendance")]
    public async Task<ActionResult<AttendanceDashboardResponse>>
        GetAttendanceDashboard()
    {
        var result =
            await _service.GetAttendanceDashboardAsync();

        return Ok(result);
    }
}