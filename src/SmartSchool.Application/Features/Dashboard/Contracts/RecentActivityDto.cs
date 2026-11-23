namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class RecentActivityDto
{
    /// <summary>
    /// student_created | violation_added
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Waktu kejadian (UTC). Teks "10 menit lalu" dihitung di FE.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Null jika pelaku tidak tercatat (CreatedBy kosong).
    /// </summary>
    public string? ActorName { get; set; }

    public string? ActorRole { get; set; }
}
