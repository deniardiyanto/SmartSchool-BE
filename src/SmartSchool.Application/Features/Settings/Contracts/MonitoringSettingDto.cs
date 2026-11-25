namespace SmartSchool.Application.Features.Settings.Contracts;

public class MonitoringSettingDto
{
    public int WarningPoint { get; set; }

    public int CoachingPoint { get; set; }

    /// <summary>
    /// True bila nilai berasal dari bawaan (belum pernah disimpan).
    /// </summary>
    public bool IsDefault { get; set; }
}
