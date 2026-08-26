using SmartSchool.Application.Common.Models;
using SmartSchool.Application.Features.Master.Students.Import.Contracts;

namespace SmartSchool.Application.Features.Master.Students.Import.Interfaces;

public interface IStudentImportService
{
    Task<ImportStudentResponse> ImportAsync(
        FileUpload file,
        string academicYear,
        CancellationToken cancellationToken = default);
}