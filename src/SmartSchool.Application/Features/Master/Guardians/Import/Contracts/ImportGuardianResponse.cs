namespace SmartSchool.Application.Features.Master.Guardians.Import.Contracts;

public class ImportGuardianResponse
{
    public int TotalRows { get; set; }

    public int SuccessRows { get; set; }

    public int FailedRows { get; set; }

    public List<ImportGuardianRowResult> Results { get; set; } = new();
}