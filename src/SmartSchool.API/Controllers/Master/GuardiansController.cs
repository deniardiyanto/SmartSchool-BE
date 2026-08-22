using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.API.Responses;
using SmartSchool.Application.Features.Guardians.Contracts;
using SmartSchool.Application.Features.Guardians.Interfaces;
using SmartSchool.API.Requests.Master.Guardians;
using ApplicationFileUpload = SmartSchool.Application.Common.Models.FileUpload;
using SmartSchool.Application.Features.Master.Guardians.Import.Contracts;
using SmartSchool.Application.Features.Master.Guardians.Import.Interfaces;

namespace SmartSchool.API.Controllers.Master;

[ApiController]
[Route("api/master/guardians")]
public class GuardiansController : ControllerBase
{
    private readonly IGuardianService _service;
private readonly IGuardianImportService _importService;

    public GuardiansController(IGuardianService service, IGuardianImportService importService)
    {
        _service = service;
        _importService = importService;
    }

    /// <summary>
    /// Get all guardians with paging & filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedGuardianResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GuardianFilterRequest request)
    {
        var result = await _service.GetPagedAsync(request);

        return Ok(ApiResponse<PagedGuardianResponse>.Ok(
            result,
            "Guardians retrieved successfully."));
    }

    /// <summary>
    /// Get guardian by Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GuardianDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        return Ok(ApiResponse<GuardianDto>.Ok(
            result,
            "Guardian retrieved successfully."));
    }

    /// <summary>
    /// Create new guardian.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGuardianRequest request)
    {
        var id = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<Guid>.Ok(
                id,
                "Guardian created successfully."));
    }

     /// <summary>
    /// Import classrooms from Excel.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(ApiResponse<ImportGuardianResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ImportGuardianResponse>),
        StatusCodes.Status400BadRequest)]
   public async Task<IActionResult> Import(
    [FromForm] ImportGuardianRequest request,
    CancellationToken cancellationToken)
    {
       if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(
                ApiResponse<ImportGuardianResponse>.Fail(
                    "File Excel wajib diupload."));
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
                cancellationToken);

        if (result.FailedRows > 0)
        {
            return BadRequest(
                ApiResponse<ImportGuardianResponse>.Fail(
                    "Import guardian gagal."));
        }

        return Ok(
            ApiResponse<ImportGuardianResponse>.Ok(
                result,
                "Guardian berhasil diimport."));
    }


    /// <summary>
    /// Update guardian.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGuardianRequest request)
    {
        await _service.UpdateAsync(id, request);

        return Ok(ApiResponse<object>.Ok(
            "Updated",
            "Guardian updated successfully."));
    }

    /// <summary>
    /// Soft delete guardian.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);

        return Ok(ApiResponse<object>.Ok(
            "Deleted",
            "Guardian deleted successfully."));
    }
}