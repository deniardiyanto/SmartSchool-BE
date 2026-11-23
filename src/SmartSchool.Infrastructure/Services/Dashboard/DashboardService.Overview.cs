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
    /// Pelanggaran = poin negatif selain keterlambatan otomatis
    /// (Reason "Late" dibuat sistem saat scan check-in).
    /// </summary>
    private IQueryable<AttendancePoint> BuildViolationQuery()
    {
        var lateReason =
            AttendancePointHelper.GetReason(
                AttendanceStatus.Late);

        return _context.AttendancePoints
            .Where(x =>
                !x.IsDeleted &&
                x.Point < 0 &&
                x.Reason != lateReason);
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

    private async Task<List<RecentActivityDto>>
        GetRecentActivitiesAsync(int limit)
    {
        var studentRows = await _context.Students
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .Select(x => new ActivityRow
            {
                Type = "student_created",

                Title = "Data siswa baru ditambahkan",

                Description = x.FullName + " (" + x.ClassRoom.Name + ")",

                OccurredAt = x.CreatedAt,

                ActorId = x.CreatedBy
            })
            .ToListAsync();

        var violationRows = await BuildViolationQuery()
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .Select(x => new ActivityRow
            {
                Type = "violation_added",

                Title = "Poin pelanggaran ditambahkan untuk " +
                        x.Student.FullName,

                Description = x.Reason,

                OccurredAt = x.CreatedAt,

                ActorId = x.CreatedBy
            })
            .ToListAsync();

        var rows = studentRows
            .Concat(violationRows)
            .OrderByDescending(x => x.OccurredAt)
            .Take(limit)
            .ToList();

        //----------------------------------------------------
        // Pelaku (hanya jika CreatedBy terisi)
        //----------------------------------------------------

        var actorIds = rows
            .Where(x => x.ActorId.HasValue)
            .Select(x => x.ActorId!.Value)
            .Distinct()
            .ToList();

        var actors = new Dictionary<Guid, (string Name, string Role)>();

        if (actorIds.Count > 0)
        {
            var users = await _context.Users
                .Where(x => actorIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    Role = x.Role.Name
                })
                .ToListAsync();

            foreach (var user in users)
            {
                actors[user.Id] = (user.FullName, user.Role);
            }
        }

        var result = new List<RecentActivityDto>();

        foreach (var row in rows)
        {
            var dto = new RecentActivityDto
            {
                Type = row.Type,

                Title = row.Title,

                Description = row.Description,

                OccurredAt = row.OccurredAt
            };

            if (row.ActorId.HasValue &&
                actors.TryGetValue(row.ActorId.Value, out var actor))
            {
                dto.ActorName = actor.Name;

                dto.ActorRole = actor.Role;
            }

            result.Add(dto);
        }

        return result;
    }

    private sealed class ActivityRow
    {
        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime OccurredAt { get; set; }

        public Guid? ActorId { get; set; }
    }
}
