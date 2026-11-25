namespace SmartSchool.Application.Common.Constants;

public static class StudentMonitoringHelper
{
    public static (string Key, string Label) GetStatus(
        int violationPoints)
    {
        return GetStatus(
            violationPoints,
            StudentMonitoringConstant.WarningPoint,
            StudentMonitoringConstant.CoachingPoint);
    }

    public static (string Key, string Label) GetStatus(
        int violationPoints,
        int warningPoint,
        int coachingPoint)
    {
        if (violationPoints >= coachingPoint)
            return ("coaching", "Pembinaan");

        if (violationPoints >= warningPoint)
            return ("warning", "Peringatan");

        return ("active", "Aktif");
    }
}
