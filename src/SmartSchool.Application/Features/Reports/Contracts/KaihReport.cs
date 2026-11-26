namespace SmartSchool.Application.Features.Reports.Contracts;

public class KaihReportResponse
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public List<KaihReportItemDto> Items { get; set; } = new();
}

public class KaihReportItemDto
{
    public Guid StudentId { get; set; }

    public string NIS { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string ClassRoomName { get; set; } = string.Empty;

    /// <summary>
    /// Jumlah hari siswa mengisi KAIH pada periode.
    /// </summary>
    public int SubmittedDays { get; set; }

    /// <summary>
    /// Habit tercapai / (hari terisi x 7), dalam persen.
    /// </summary>
    public decimal CompletionRate { get; set; }
}
