using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
//using SmartSchool.Application.Common.Models;
using SmartSchool.API.Requests.Master.ClassRooms;
using ApplicationFileUpload = SmartSchool.Application.Common.Models.FileUpload;
using SmartSchool.Application.Features.ClassRooms.Contracts;
using SmartSchool.Application.Features.ClassRooms.Interfaces;
using SmartSchool.Application.Features.Master.ClassRooms.Import.Contracts;
using SmartSchool.Application.Features.Master.ClassRooms.Import.Interfaces;

namespace SmartSchool.API.Controllers.Master;

[ApiController]
[Route("api/master/classrooms")]
public class ClassRoomsController : ControllerBase
{
    private readonly IClassRoomService _service;

    private readonly IClassRoomImportService _importService;

    public ClassRoomsController(
        IClassRoomService service,
        IClassRoomImportService importService)
    {
        _service = service;

        _importService = importService;
    }

    /// <summary>
    /// Get classrooms with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedClassRoomResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ClassRoomFilterRequest request)
    {
        var result =
            await _service.GetPagedAsync(request);

        return Ok(
            ApiResponse<PagedClassRoomResponse>.Ok(
                result));
    }

    /// <summary>
    /// Get classroom by Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<ClassRoomDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id)
    {
        var result =
            await _service.GetByIdAsync(id);

        return Ok(
            ApiResponse<ClassRoomDto>.Ok(
                result!));
    }

    /// <summary>
    /// Create new classroom.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<Guid>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassRoomRequest request)
    {
        var id =
            await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<Guid>.Ok(
                id,
                "Class room created successfully."));
    }

//     /// <summary>
//     /// Import classrooms from Excel.
//     /// </summary>
//     [HttpPost("import")]
//     [Consumes("multipart/form-data")]
//     [ProducesResponseType(
//         typeof(ApiResponse<ImportClassRoomResponse>),
//         StatusCodes.Status200OK)]
//     [ProducesResponseType(
//         typeof(ApiResponse<ImportClassRoomResponse>),
//         StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> Import(
//     [FromForm] ImportClassRoomRequest request,
//     CancellationToken cancellationToken)
//     {
//        if (request.File == null || request.File.Length == 0)
//         {
//             return BadRequest(
//                 ApiResponse<ImportClassRoomResponse>.Fail(
//                     "File Excel wajib diupload."));
//         }

//         await using var stream =
//     request.File.OpenReadStream();

//        var upload = new ApplicationFileUpload
// {
//    FileName = request.File.FileName,
// ContentType = request.File.ContentType,
//     Content = stream
// };

//         var result =
//             await _importService.ImportAsync(
//                 upload,
//                 cancellationToken);

//         if (result.FailedRows > 0)
//         {
//             return BadRequest(
//                 ApiResponse<ImportClassRoomResponse>.Fail(
//                     "Import classroom gagal."));
//         }

//         return Ok(
//             ApiResponse<ImportClassRoomResponse>.Ok(
//                 result,
//                 "Classroom berhasil diimport."));
//     }

/// <summary>
/// Import classrooms from Excel.
/// </summary>
[HttpPost("import")]
[Consumes("multipart/form-data")]
[ProducesResponseType(
    typeof(ApiResponse<ImportClassRoomResponse>),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ApiResponse<ImportClassRoomResponse>),
    StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Import(
    [FromForm] ImportClassRoomRequest request,
    CancellationToken cancellationToken)
{
    //---------------------------------------------------------
    // Validate request
    //---------------------------------------------------------

    if (request.File == null)
    {
        return BadRequest(
            ApiResponse<ImportClassRoomResponse>.Fail(
                "File Excel wajib diupload."));
    }

    if (request.File.Length == 0)
    {
        return BadRequest(
            ApiResponse<ImportClassRoomResponse>.Fail(
                "File Excel kosong."));
    }

    //---------------------------------------------------------
    // Create application file upload
    //---------------------------------------------------------

    await using var stream =
        request.File.OpenReadStream();

    var upload = new ApplicationFileUpload
    {
        FileName = request.File.FileName,
        ContentType = request.File.ContentType,
        Content = stream
    };

    //---------------------------------------------------------
    // Import
    //---------------------------------------------------------

    var result =
        await _importService.ImportAsync(
            upload,
            cancellationToken);

    //---------------------------------------------------------
    // Import failed
    //---------------------------------------------------------

    if (result.FailedRows > 0)
    {
        return BadRequest(
            ApiResponse<ImportClassRoomResponse>.Fail(
                "Import classroom gagal."));
    }

    //---------------------------------------------------------
    // Import success
    //---------------------------------------------------------

    return Ok(
        ApiResponse<ImportClassRoomResponse>.Ok(
            result,
            "Classroom berhasil diimport."));
}

    /// <summary>
    /// Update classroom.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<string>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateClassRoomRequest request)
    {
        await _service.UpdateAsync(
            id,
            request);

        return Ok(
            ApiResponse<string>.Ok(
                "Updated",
                "Class room updated successfully."));
    }

    /// <summary>
    /// Soft delete classroom.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<string>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        await _service.DeleteAsync(id);

        return Ok(
            ApiResponse<string>.Ok(
                "Deleted",
                "Class room deleted successfully."));
    }
}