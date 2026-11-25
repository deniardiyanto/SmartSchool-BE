using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
using SmartSchool.Application.Features.Attendances.CloseDay.Contracts;
using SmartSchool.Application.Features.Attendances.CloseDay.Interfaces;

namespace SmartSchool.API.Controllers;

[ApiController]
[Route("api/attendance/close-day")]
public class AttendanceCloseDayController : ControllerBase
{
    private readonly IAttendanceCloseDayService _service;

    public AttendanceCloseDayController(
        IAttendanceCloseDayService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tutup hari: tandai Alpa siswa aktif yang belum punya presensi.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<CloseDayResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloseDay(
        [FromBody] CloseDayRequest? request)
    {
        var result = await _service.CloseDayAsync(
            request ?? new CloseDayRequest());

        return Ok(ApiResponse<CloseDayResponse>.Ok(
            result,
            result.MarkedAbsent > 0
                ? $"{result.MarkedAbsent} siswa ditandai alpa."
                : "Tidak ada siswa yang perlu ditandai alpa."));
    }
}
