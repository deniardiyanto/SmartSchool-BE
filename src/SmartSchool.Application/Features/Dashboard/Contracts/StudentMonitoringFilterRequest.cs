namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class StudentMonitoringFilterRequest
{
    public string? Keyword { get; set; }

    public Guid? ClassRoomId { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
