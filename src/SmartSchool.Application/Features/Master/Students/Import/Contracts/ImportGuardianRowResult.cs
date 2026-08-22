namespace SmartSchool.Application.Features.Master.Students.Import.Contracts;

public class ImportStudentRowResult
{
    public int RowNumber { get; set; }

    public bool Success { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? StudentId { get; set; }
}