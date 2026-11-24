using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Features.Dashboard.Contracts;
using SmartSchool.Application.Features.Dashboard.Interfaces;
using SmartSchool.Domain.Enums;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Dashboard;

public partial class DashboardService : IDashboardService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DashboardService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AttendanceDashboardResponse>
        GetAttendanceDashboardAsync()
    {
        var today = DateOnly.FromDateTime(
            _dateTimeProvider.UtcNow);

        //----------------------------------------------------
        // Attendance Today
        //----------------------------------------------------

        var attendances = _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate == today);

        //----------------------------------------------------
        // Present
        //----------------------------------------------------

        var present = await attendances
            .CountAsync(x =>
                x.Status == AttendanceStatus.Present);

        //----------------------------------------------------
        // Late
        //----------------------------------------------------

        var late = await attendances
            .CountAsync(x =>
                x.Status == AttendanceStatus.Late);

        //----------------------------------------------------
        // Absent
        //----------------------------------------------------

        var absent = await attendances
            .CountAsync(x =>
                x.Status == AttendanceStatus.Absent);

        //----------------------------------------------------
        // Checked Out
        //----------------------------------------------------

        var checkedOut = await attendances
            .CountAsync(x =>
                x.CheckOutTime != null);

        //----------------------------------------------------
        // Total Attendance
        //----------------------------------------------------

        var totalAttendance = await attendances
            .CountAsync();

        //----------------------------------------------------
        // Total Point
        //----------------------------------------------------

        var totalPoint = await _context.AttendancePoints
            .Where(x =>
                !x.IsDeleted &&
                x.PointDate.Date ==
                _dateTimeProvider.UtcNow.Date)
            .SumAsync(x => (int?)x.Point) ?? 0;
        var totalStudent = await _context.Students
    .CountAsync(x =>
        x.IsActive &&
        !x.IsDeleted);
        decimal attendanceRate = 0;

        if (totalStudent > 0)
        {
            attendanceRate =
                Math.Round(
                    (decimal)totalAttendance /
                    totalStudent * 100,
                    2);
        }
        var startDate = today.AddDays(-6);

        var weeklyTrend = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate >= startDate &&
                x.AttendanceDate <= today)
            .GroupBy(x => x.AttendanceDate)
            .Select(g => new AttendanceTrendDto
            {
                Date = g.Key,

                Present = g.Count(x =>
                    x.Status == AttendanceStatus.Present),

                Late = g.Count(x =>
                    x.Status == AttendanceStatus.Late),

                Absent = g.Count(x =>
                    x.Status == AttendanceStatus.Absent)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var monthStart = today.AddDays(-29);

        var monthlyTrend = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate >= monthStart &&
                x.AttendanceDate <= today)
            .GroupBy(x => x.AttendanceDate)
            .Select(g => new AttendanceTrendDto
            {
                Date = g.Key,

                Present = g.Count(x =>
                    x.Status == AttendanceStatus.Present),

                Late = g.Count(x =>
                    x.Status == AttendanceStatus.Late),

                Absent = g.Count(x =>
                    x.Status == AttendanceStatus.Absent)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
        var statusSummary =
        new AttendanceStatusSummaryDto
        {
            Present = present,

            Late = late,

            Absent = absent
        };
        // Poin = poin pelanggaran, jadi siswa terbaik adalah
        // yang total poinnya paling rendah.

        // Top Student
        var topStudents = await BuildStudentPointRankingQuery()
            .OrderBy(x => x.TotalPoint)
            .Take(10)
            .ToListAsync();

        // Bottom Student
        var bottomStudents = await BuildStudentPointRankingQuery()
            .OrderByDescending(x => x.TotalPoint)
            .Take(10)
            .ToListAsync();
        var whatsApp =
await GetWhatsAppAnalyticsAsync();

        var scanPerHour =
            await GetScanPerHourAsync();

        //----------------------------------------------------
        // Response
        //----------------------------------------------------

        return new AttendanceDashboardResponse
        {
            Date = today,

            Present = present,

            Late = late,

            Absent = absent,

            CheckedOut = checkedOut,

            TotalAttendance = totalAttendance,

            TotalPoint = totalPoint,

            TotalStudent = totalStudent,

            AttendanceRate = attendanceRate,

            WeeklyTrend = weeklyTrend,

            MonthlyTrend = monthlyTrend,

            StatusSummary = statusSummary,

            TopStudents = topStudents,

            BottomStudents = bottomStudents,
            WhatsAppAnalytics = whatsApp,

            ScanPerHour = scanPerHour
        };
    }
    // ===============================
    // Helper Method
    // ===============================
    private IQueryable<StudentPointRankingDto>
        BuildStudentPointRankingQuery()
    {
        return _context.AttendancePoints
            .Where(x => !x.IsDeleted)
            .GroupBy(x => new
            {
                x.StudentId,
                x.Student.FullName,
                ClassRoom = x.Student.ClassRoom.Name
            })
            .Select(g => new StudentPointRankingDto
            {
                StudentId = g.Key.StudentId,

                StudentName = g.Key.FullName,

                ClassRoom = g.Key.ClassRoom,

                TotalPoint = g.Sum(x => x.Point)
            });
    }
    private async Task<WhatsAppAnalyticsDto>
    GetWhatsAppAnalyticsAsync()
    {
        var total =
            await _context.WhatsAppLogs
                .CountAsync(x => !x.IsDeleted);

        var success =
            await _context.WhatsAppLogs
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "Success");

        var failed =
            await _context.WhatsAppLogs
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "Failed");

        var pending =
            await _context.WhatsAppLogs
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "Pending");

        decimal rate = 0;

        if (total > 0)
        {
            rate = Math.Round(
                (decimal)success / total * 100,
                2);
        }

        return new WhatsAppAnalyticsDto
        {
            Success = success,

            Failed = failed,

            Pending = pending,

            SuccessRate = rate
        };
    }
    private async Task<List<ScanPerHourDto>>
        GetScanPerHourAsync()
    {
        var today =
            DateOnly.FromDateTime(
                _dateTimeProvider.UtcNow);

        return await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate == today)
            .GroupBy(x =>
                x.CheckInTime!.Value.Hour)
            .Select(g => new ScanPerHourDto
            {
                Hour = g.Key,

                TotalScan = g.Count()
            })
            .OrderBy(x => x.Hour)
            .ToListAsync();
    }
}