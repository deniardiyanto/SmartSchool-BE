using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
using SmartSchool.Application.Features.Students.Contracts;
using SmartSchool.Application.Features.Students.Interfaces;
using SmartSchool.API.Requests.Master.Students;
using ApplicationFileUpload = SmartSchool.Application.Common.Models.FileUpload;
using SmartSchool.Application.Features.Master.Students.Import.Contracts;
using SmartSchool.Application.Features.Master.Students.Import.Interfaces;


namespace SmartSchool.API.Controllers.Master;

[ApiController]
[Route("api/master/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    private readonly IStudentImportService _importService;

    public StudentsController(IStudentService service, IStudentImportService importService)
    {
        _service = service;
        _importService = importService;
    }

    /// <summary>
    /// Get all students with paging & filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedStudentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] StudentFilterRequest request)
    {
        var result = await _service.GetPagedAsync(request);

        return Ok(ApiResponse<PagedStudentResponse>.Ok(
            result,
            "Students retrieved successfully."));
    }

    /// <summary>
    /// Get student by Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        return Ok(ApiResponse<StudentDto>.Ok(
            result,
            "Student retrieved successfully."));
    }

    /// <summary>
    /// Create new student.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudentRequest request)
    {
        var id = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<Guid>.Ok(
                id,
                "Student created successfully."));
    }

    /// <summary>
    /// Import classrooms from Excel.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(ApiResponse<ImportStudentResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ImportStudentResponse>),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
    [FromForm] ImportStudentRequest request, [FromQuery] string academicYear,
    CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(
                ApiResponse<ImportStudentResponse>.Fail(
                    "File Excel wajib diupload."));
        }
        if (string.IsNullOrWhiteSpace(academicYear))
    {
        return BadRequest("Academic Year wajib diisi.");
    }

        await using var stream =
    request.File.OpenReadStream();

        var upload = new ApplicationFileUpload
        {
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            Content = stream
        };

        var result =
            await _importService.ImportAsync(
                upload,
                academicYear,
                cancellationToken);

        // if (result.FailedRows > 0)
        // {
        //     return BadRequest(
        //         ApiResponse<ImportGuardianResponse>.Fail(
        //             "Import guardian gagal."));
        // }
        if (result.FailedRows > 0)
        {
            return BadRequest(result);
        }

        return Ok(
            ApiResponse<ImportStudentResponse>.Ok(
                result,
                "Student berhasil diimport."));
    }


    /// <summary>
    /// Update student.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateStudentRequest request)
    {
        await _service.UpdateAsync(id, request);

        return Ok(ApiResponse<object>.Ok(
            "Updated",
            "Student updated successfully."));
    }

    /// <summary>
    /// Soft delete student.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);

        return Ok(ApiResponse<object>.Ok(
            "Deleted",
            "Student deleted successfully."));
    }
}