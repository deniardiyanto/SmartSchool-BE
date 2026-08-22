using Microsoft.AspNetCore.Http;

namespace SmartSchool.API.Requests.Master.Guardians;

public class ImportGuardianRequest
{
    public IFormFile File { get; set; } = null!;
}