using SmartSchool.Application.Features.Reports.Contracts;

namespace SmartSchool.Application.Features.Reports.Interfaces;

public interface IReportService
{
    Task<AttendanceReportResponse> GetAttendanceReportAsync(
        ReportFilterRequest request);

    Task<ViolationReportResponse> GetViolationReportAsync(
        ReportFilterRequest request);

    Task<KaihReportResponse> GetKaihReportAsync(
        ReportFilterRequest request);
}
