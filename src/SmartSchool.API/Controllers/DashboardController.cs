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