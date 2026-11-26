using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Features.Reports.Contracts;
using SmartSchool.Application.Features.Reports.Interfaces;
using SmartSchool.Domain.Enums;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Reports;

public class ReportService : IReportService
{
    private const int KaihHabitCount = 7;

    private const int MaxRangeDays = 366;

    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReportService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    //--------------------------------------------------------
    // Presensi
    //--------------------------------------------------------

    public async Task<AttendanceReportResponse> GetAttendanceReportAsync(
        ReportFilterRequest request)
    {
        var (from, to) = ResolveRange(request);

        var students = await GetStudentsAsync(request.ClassRoomId);

        var effectiveDays = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate >= from &&
                x.AttendanceDate <= to)
            .Select(x => x.AttendanceDate)
            .Distinct()
            .CountAsync();

        var counts = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate >= from &&
                x.AttendanceDate <= to &&
                x.Student.IsActive &&
                !x.Student.IsDeleted &&
                (request.ClassRoomId == null ||
                 x.Student.ClassRoomId == request.ClassRoomId))
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,

                Present = g.Sum(x =>
                    x.Status == AttendanceStatus.Present ? 1 : 0),

                Late = g.Sum(x =>
                    x.Status == AttendanceStatus.Late ? 1 : 0),

                Sick = g.Sum(x =>
                    x.Status == AttendanceStatus.Sick ? 1 : 0),

                Permission = g.Sum(x =>
                    x.Status == AttendanceStatus.Permission ? 1 : 0),

                Absent = g.Sum(x =>
                    x.Status == AttendanceStatus.Absent ? 1 : 0)
            })
            .ToDictionaryAsync(x => x.StudentId);

        var items = students
            .Select(s =>
            {
                counts.TryGetValue(s.Id, out var c);

                var present = c?.Present ?? 0;
                var late = c?.Late ?? 0;

                return new AttendanceReportItemDto
                {
                    StudentId = s.Id,
                    NIS = s.NIS,
                    FullName = s.FullName,
                    ClassRoomName = s.ClassRoomName,
                    Present = present,
                    Late = late,
                    Sick = c?.Sick ?? 0,
                    Permission = c?.Permission ?? 0,
                    Absent = c?.Absent ?? 0,
                    AttendanceRate = Percent(present + late, effectiveDays)
                };
            })
            .ToList();

        return new AttendanceReportResponse
        {
            From = from,
            To = to,
            EffectiveDays = effectiveDays,
            Items = items
        };
    }

    //--------------------------------------------------------
    // Poin pelanggaran
    //--------------------------------------------------------

    public async Task<ViolationReportResponse> GetViolationReportAsync(
        ReportFilterRequest request)
    {
        var (from, to) = ResolveRange(request);

        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var toUtcExclusive = to.AddDays(1)
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var students = await GetStudentsAsync(request.ClassRoomId);

        var totals = await _context.AttendancePoints
            .Where(x =>
                !x.IsDeleted &&
                x.PointDate >= fromUtc &&
                x.PointDate < toUtcExclusive &&
                x.Student.IsActive &&
                !x.Student.IsDeleted &&
                (request.ClassRoomId == null ||
                 x.Student.ClassRoomId == request.ClassRoomId))
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,

                Incidents = g.Sum(x => x.Point > 0 ? 1 : 0),

                Total = g.Sum(x => x.Point)
            })
            .ToDictionaryAsync(x => x.StudentId);

        // Total dibatasi minimal 0 di memori (koreksi tidak membuat negatif).
        var items = students
            .Where(s => totals.ContainsKey(s.Id))
            .Select(s => new ViolationReportItemDto
            {
                StudentId = s.Id,
                NIS = s.NIS,
                FullName = s.FullName,
                ClassRoomName = s.ClassRoomName,
                Incidents = totals[s.Id].Incidents,
                TotalPoint = Math.Max(totals[s.Id].Total, 0)
            })
            .Where(x => x.TotalPoint > 0)
            .OrderByDescending(x => x.TotalPoint)
            .ThenBy(x => x.FullName)
            .ToList();

        return new ViolationReportResponse
        {
            From = from,
            To = to,
            Items = items
        };
    }

    //--------------------------------------------------------
    // KAIH
    //--------------------------------------------------------

    public async Task<KaihReportResponse> GetKaihReportAsync(
        ReportFilterRequest request)
    {
        var (from, to) = ResolveRange(request);

        var students = await GetStudentsAsync(request.ClassRoomId);

        var entries = await _context.KaihEntries
            .Where(x =>
                !x.IsDeleted &&
                x.EntryDate >= from &&
                x.EntryDate <= to &&
                x.Student.IsActive &&
                !x.Student.IsDeleted &&
                (request.ClassRoomId == null ||
                 x.Student.ClassRoomId == request.ClassRoomId))
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,

                Days = g.Count(),

                Habits = g.Sum(x =>
                    (x.WakeUpEarly ? 1 : 0) +
                    (x.Worship ? 1 : 0) +
                    (x.Exercise ? 1 : 0) +
                    (x.HealthyEating ? 1 : 0) +
                    (x.Learning ? 1 : 0) +
                    (x.Socializing ? 1 : 0) +
                    (x.EarlySleep ? 1 : 0))
            })
            .ToDictionaryAsync(x => x.StudentId);

        var items = students
            .Select(s =>
            {
                entries.TryGetValue(s.Id, out var e);

                return new KaihReportItemDto
                {
                    StudentId = s.Id,
                    NIS = s.NIS,
                    FullName = s.FullName,
                    ClassRoomName = s.ClassRoomName,
                    SubmittedDays = e?.Days ?? 0,
                    CompletionRate = e == null
                        ? 0m
                        : Percent(e.Habits, e.Days * KaihHabitCount)
                };
            })
            .ToList();

        return new KaihReportResponse
        {
            From = from,
            To = to,
            Items = items
        };
    }

    //--------------------------------------------------------
    // Helper
    //--------------------------------------------------------

    private (DateOnly From, DateOnly To) ResolveRange(
        ReportFilterRequest request)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var from = request.From ?? new DateOnly(today.Year, today.Month, 1);

        var to = request.To ?? today;

        if (from > to)
            throw new BadRequestException(
                "Tanggal awal tidak boleh setelah tanggal akhir.");

        if (to.DayNumber - from.DayNumber > MaxRangeDays)
            throw new BadRequestException(
                $"Rentang periode maksimal {MaxRangeDays} hari.");

        return (from, to);
    }

    private async Task<List<StudentRow>> GetStudentsAsync(Guid? classRoomId)
    {
        var query = _context.Students
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted);

        if (classRoomId.HasValue)
            query = query.Where(x => x.ClassRoomId == classRoomId.Value);

        return await query
            .OrderBy(x => x.ClassRoom.Name)
            .ThenBy(x => x.FullName)
            .Select(x => new StudentRow
            {
                Id = x.Id,
                NIS = x.NIS,
                FullName = x.FullName,
                ClassRoomName = x.ClassRoom.Name
            })
            .ToListAsync();
    }

    private static decimal Percent(int part, int total)
    {
        if (total <= 0)
            return 0m;

        return Math.Round(part * 100m / total, 1);
    }

    private sealed class StudentRow
    {
        public Guid Id { get; set; }

        public string NIS { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;
    }
}
