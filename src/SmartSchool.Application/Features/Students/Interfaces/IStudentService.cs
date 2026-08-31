using SmartSchool.Application.Features.Students.Contracts;

namespace SmartSchool.Application.Features.Students.Interfaces;

public interface IStudentService
{
    Task<PagedStudentResponse> GetPagedAsync(StudentFilterRequest request);

    Task<StudentDto> GetByIdAsync(Guid id);

    Task<Guid> CreateAsync(CreateStudentRequest request,
    CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, UpdateStudentRequest request);

    Task DeleteAsync(Guid id);
}