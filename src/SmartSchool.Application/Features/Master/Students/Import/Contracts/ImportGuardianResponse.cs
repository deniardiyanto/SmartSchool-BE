namespace SmartSchool.Application.Features.Master.Students.Import.Contracts;

public class ImportStudentResponse
{
    public int TotalRows { get; set; }

    public int SuccessRows { get; set; }

    public int FailedRows { get; set; }

    public List<ImportStudentRowResult> Results { get; set; } = new();
}