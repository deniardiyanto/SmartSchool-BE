using Microsoft.AspNetCore.Http;

namespace SmartSchool.API.Requests.Master.Students;

public class ImportStudentRequest
{
    public IFormFile File { get; set; } = null!;
}