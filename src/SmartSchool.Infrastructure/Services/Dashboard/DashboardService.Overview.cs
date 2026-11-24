using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Constants;
using SmartSchool.Application.Features.Dashboard.Contracts;
using SmartSchool.Domain.Entities;
using SmartSchool.Domain.Enums;

namespace SmartSchool.Infrastructure.Services.Dashboard;

/// <summary>
/// Dashboard overview untuk halaman admin CMS.
/// </summary>
public partial class DashboardService
{
    private const int DefaultActivityLimit = 5;

    private const int MaxActivityLimit = 20;

    // 7 Kebiasaan Anak Indonesia Hebat
    private const int KaihHabitCount = 7;

    public async Task<DashboardOverviewResponse>
        GetOverviewAsync(int activityLimit)
    {
        // Tanggal memakai UTC, konsisten dengan cara
        // AttendanceScannerService menyimpan AttendanceDate.
        var now = _dateTimeProvider.UtcNow;

        var today = DateOnly.FromDateTime(now);

        var yesterday = today.AddDays(-1);

        var limit = activityLimit <= 0
            ? DefaultActivityLimit
            : Math.Min(activityLimit, MaxActivityLimit);

        var monthStart = new DateTime(
            now.Year,
            now.Month,
            1,
            0, 0, 0,
            DateTimeKind.Utc);

        var previousMonthStart = monthStart.AddMonths(-1);

        //----------------------------------------------------
        // Total Siswa
        //----------------------------------------------------

        var totalStudent = await _context.Students
            .CountAsync(x =>
                x.IsActive &&
                !x.IsDeleted);

        var newStudents = await _context.Students
            .CountAsync(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.EnrollmentDate >= monthStart);

        //----------------------------------------------------
        // Presensi Hari Ini (per status)
        //----------------------------------------------------

        var todayCounts = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate == today &&
                x.Student.IsActive &&
                !x.Student.IsDeleted)
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                Status = g.Key,
                Total = g.Count()
            })
            .ToListAsync();

        int CountOf(AttendanceStatus status) =>
            todayCounts
                .Where(x => x.Status == status)
                .Sum(x => x.Total);

        var present = CountOf(AttendanceStatus.Present);

        var late = CountOf(AttendanceStatus.Late);

        var sick = CountOf(AttendanceStatus.Sick);

        var permission = CountOf(AttendanceStatus.Permission);

        var absentRecorded = CountOf(AttendanceStatus.Absent);

        var recorded = todayCounts.Sum(x => x.Total);

        // Siswa aktif tanpa data presensi hari ini dihitung Alpa,
        // sehingga seluruh item rekap berjumlah total siswa.
        var notRecorded = Math.Max(totalStudent - recorded, 0);

        var absent = absentRecorded + notRecorded;

        //----------------------------------------------------
        // Kehadiran Hari Ini vs Kemarin
        //----------------------------------------------------

        var attended = present + late;

        var rate = Percent(attended, totalStudent);

        decimal? changeFromYesterday = null;

        var yesterdayRecorded = await _context.Attendances
            .CountAsync(x =>
                !x.IsDeleted &&
                x.AttendanceDate == yesterday &&
                x.Student.IsActive &&
                !x.Student.IsDeleted);

        if (yesterdayRecorded > 0)
        {
            var yesterdayAttended = await _context.Attendances
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.AttendanceDate == yesterday &&
                    x.Student.IsActive &&
                    !x.Student.IsDeleted &&
                    (x.Status == AttendanceStatus.Present ||
                     x.Status == AttendanceStatus.Late));

            changeFromYesterday = Math.Round(
                rate - Percent(yesterdayAttended, totalStudent),
                1);
        }

        //----------------------------------------------------
        // Pelanggaran Bulan Ini vs Bulan Lalu
        //----------------------------------------------------

        var violationsThisMonth = await BuildViolationQuery()
            .CountAsync(x =>
                x.PointDate >= monthStart);

        var violationsLastMonth = await BuildViolationQuery()
            .CountAsync(x =>
                x.PointDate >= previousMonthStart &&
                x.PointDate < monthStart);

        //----------------------------------------------------
        // Response
        //----------------------------------------------------

        return new DashboardOverviewResponse
        {
            Date = today,

            Students = new StudentSummaryDto
            {
                Total = totalStudent,

                NewThisMonth = newStudents
            },

            AttendanceToday = new AttendanceRateSummaryDto
            {
                Rate = rate,

                AttendedCount = attended,

                ChangeFromYesterday = changeFromYesterday
            },

            Violations = new ViolationSummaryDto
            {
                ThisMonth = violationsThisMonth,

                LastMonth = violationsLastMonth,

                ChangeFromLastMonth =
                    violationsThisMonth - violationsLastMonth
            },

            Kaih = await GetKaihSummaryAsync(today, totalStudent),

            PresenceRecap = new PresenceRecapDto
            {
                Total = totalStudent,

                NotRecorded = notRecorded,

                Items = new List<PresenceRecapItemDto>
                {
                    BuildRecapItem("present", "Hadir", present, totalStudent),

                    BuildRecapItem("sick", "Sakit", sick, totalStudent),

                    BuildRecapItem("permission", "Izin", permission, totalStudent),

                    BuildRecapItem("absent", "Alpa", absent, totalStudent),

                    BuildRecapItem("late", "Terlambat", late, totalStudent)
                }
            },

            RecentActivities = await GetRecentActivitiesAsync(limit)
        };
    }

    // ===============================
    // Helper Method (Overview)
    // ===============================

    /// <summary>
    /// Pelanggaran = catatan poin bernilai positif (poin pelanggaran),
    /// termasuk keterlambatan yang dibuat otomatis saat scan check-in.
    /// Nilai negatif adalah koreksi dan tidak dihitung sebagai pelanggaran.
    /// </summary>
    private IQueryable<AttendancePoint> BuildViolationQuery()
    {
        return _context.AttendancePoints
            .Where(x =>
                !x.IsDeleted &&
                x.Point > 0);
    }

    private static PresenceRecapItemDto BuildRecapItem(
        string key,
        string label,
        int count,
        int total)
    {
        return new PresenceRecapItemDto
        {
            Key = key,

            Label = label,

            Count = count,

            Percentage = Percent(count, total)
        };
    }

    private static decimal Percent(int part, int total)
    {
        if (total <= 0)
            return 0;

        return Math.Round(
            (decimal)part / total * 100,
            1);
    }

    /// <summary>
    /// Aktivitas terbaru dibaca dari tabel activity_logs
    /// (diisi oleh IActivityLogService saat aksi terjadi).
    /// </summary>
    private async Task<List<RecentActivityDto>>
        GetRecentActivitiesAsync(int limit)
    {
        return await _context.ActivityLogs
            .OrderByDescending(x => x.OccurredAt)
            .Take(limit)
            .Select(x => new RecentActivityDto
            {
                Type = x.Type,

                Title = x.Title,

                Description = x.Description,

                OccurredAt = x.OccurredAt,

                ActorName = x.ActorUser == null
                    ? null
                    : x.ActorUser.FullName,

                ActorRole = x.ActorUser == null
                    ? null
                    : x.ActorUser.Role.Name
            })
            .ToListAsync();
    }

    /// <summary>
    /// KAIH Terkumpul = siswa aktif yang sudah mengisi 7 kebiasaan
    /// hari ini dibagi total siswa aktif.
    /// </summary>
    private async Task<KaihSummaryDto> GetKaihSummaryAsync(
        DateOnly today,
        int totalStudent)
    {
        var lastWeek = today.AddDays(-7);

        var todayEntries = _context.KaihEntries
            .Where(x =>
                !x.IsDeleted &&
                x.EntryDate == today &&
                x.Student.IsActive &&
                !x.Student.IsDeleted);

        var submitted = await todayEntries.CountAsync();

        var averageHabits = await todayEntries
            .Select(x => (double?)(
                (x.WakeUpEarly ? 1 : 0) +
                (x.Worship ? 1 : 0) +
                (x.Exercise ? 1 : 0) +
                (x.HealthyEating ? 1 : 0) +
                (x.Learning ? 1 : 0) +
                (x.Socializing ? 1 : 0) +
                (x.EarlySleep ? 1 : 0)))
            .AverageAsync();

        var rate = Percent(submitted, totalStudent);

        decimal? changeFromLastWeek = null;

        var lastWeekSubmitted = await _context.KaihEntries
            .CountAsync(x =>
                !x.IsDeleted &&
                x.EntryDate == lastWeek &&
                x.Student.IsActive &&
                !x.Student.IsDeleted);

        if (lastWeekSubmitted > 0)
        {
            changeFromLastWeek = Math.Round(
                rate - Percent(lastWeekSubmitted, totalStudent),
                1);
        }

        return new KaihSummaryDto
        {
            Rate = rate,

            SubmittedCount = submitted,

            AverageHabitCompletion = Math.Round(
                (decimal)(averageHabits ?? 0) / KaihHabitCount * 100,
                1),

            ChangeFromLastWeek = changeFromLastWeek
        };
    }
}
