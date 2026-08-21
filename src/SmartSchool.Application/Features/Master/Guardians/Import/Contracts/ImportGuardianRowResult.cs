namespace SmartSchool.Application.Features.Master.Guardians.Import.Contracts;

public class ImportGuardianRowResult
{
    public int RowNumber { get; set; }

    public bool Success { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? GuardianId { get; set; }
}