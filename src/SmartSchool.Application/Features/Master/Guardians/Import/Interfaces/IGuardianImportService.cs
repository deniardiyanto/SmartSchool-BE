using SmartSchool.Application.Common.Models;
using SmartSchool.Application.Features.Master.Guardians.Import.Contracts;

namespace SmartSchool.Application.Features.Master.Guardians.Import.Interfaces;

public interface IGuardianImportService
{
    Task<ImportGuardianResponse> ImportAsync(
        FileUpload file,
        CancellationToken cancellationToken = default);
}