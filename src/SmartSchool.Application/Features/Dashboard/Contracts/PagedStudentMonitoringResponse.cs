namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class PagedStudentMonitoringResponse
{
    public IReadOnlyList<StudentMonitoringItemDto> Items { get; set; }
        = new List<StudentMonitoringItemDto>();

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }
}
