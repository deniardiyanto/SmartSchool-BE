namespace SmartSchool.Application.Features.Dashboard.Contracts;

public class StudentSummaryDto
{
    /// <summary>
    /// Total siswa aktif (Total Peserta Didik).
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Siswa yang masuk (EnrollmentDate) pada bulan berjalan.
    /// </summary>
    public int NewThisMonth { get; set; }
}
