namespace SmartSchool.Application.Features.Settings.Contracts;

public class SaveAttendanceRuleRequest
{
    public string RuleName { get; set; } = string.Empty;

    public TimeOnly CheckInStart { get; set; }

    public TimeOnly CheckInEnd { get; set; }

    public TimeOnly CheckOutStart { get; set; }

    public TimeOnly CheckOutEnd { get; set; }

    public int LateToleranceMinutes { get; set; }

    public int PresentPoint { get; set; }

    public int LatePoint { get; set; }

    public int AbsentPoint { get; set; }

    public int EarlyCheckoutPoint { get; set; }
}
