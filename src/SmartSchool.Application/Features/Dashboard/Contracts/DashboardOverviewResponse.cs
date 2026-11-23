namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class DashboardOverviewResponse
{
    public DateOnly Date { get; set; }

    public StudentSummaryDto Students { get; set; } = new();

    public AttendanceRateSummaryDto AttendanceToday { get; set; } = new();

    public ViolationSummaryDto Violations { get; set; } = new();

    public PresenceRecapDto PresenceRecap { get; set; } = new();

    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}
