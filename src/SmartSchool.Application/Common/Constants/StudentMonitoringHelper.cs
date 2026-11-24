namespace SmartSchool.Application.Common.Constants;

public static class StudentMonitoringHelper
{
    public static (string Key, string Label) GetStatus(
        int violationPoints)
    {
        if (violationPoints >= StudentMonitoringConstant.CoachingPoint)
            return ("coaching", "Pembinaan");

        if (violationPoints >= StudentMonitoringConstant.WarningPoint)
            return ("warning", "Peringatan");

        return ("active", "Aktif");
    }
}
