using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Constants;
using SmartSchool.Application.Features.Dashboard.Contracts;
using SmartSchool.Domain.Enums;

namespace SmartSchool.Infrastructure.Services.Dashboard;

/// <summary>
/// Widget "Kelas Terbaik Hari Ini" dan "Monitoring Peserta Didik".
/// </summary>
public partial class DashboardService
{
    private const int DefaultBestClassLimit = 3;

    private const int MaxBestClassLimit = 10;

    private const int MaxMonitoringPageSize = 100;

    public async Task<List<BestClassDto>> GetBestClassesAsync(int top)
    {
        var limit = top <= 0
            ? DefaultBestClassLimit
            : Math.Min(top, MaxBestClassLimit);

        var today = DateOnly.FromDateTime(
            _dateTimeProvider.UtcNow);

        //----------------------------------------------------
        // Siswa aktif per kelas
        //----------------------------------------------------

        var classes = await _context.Students
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.ClassRoom.IsActive &&
                !x.ClassRoom.IsDeleted)
            .GroupBy(x => new
            {
                x.ClassRoomId,
                x.ClassRoom.Name
            })
            .Select(g => new
            {
                ClassRoomId = g.Key.ClassRoomId,

                ClassRoomName = g.Key.Name,

                TotalStudents = g.Count()
            })
            .ToListAsync();

        //----------------------------------------------------
        // Hadir + Terlambat hari ini per kelas
        //----------------------------------------------------

        var attended = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate == today &&
                x.Student.IsActive &&
                !x.Student.IsDeleted &&
                (x.Status == AttendanceStatus.Present ||
                 x.Status == AttendanceStatus.Late))
            .GroupBy(x => x.Student.ClassRoomId)
            .Select(g => new
            {
                ClassRoomId = g.Key,

                Total = g.Count()
            })
            .ToDictionaryAsync(
                x => x.ClassRoomId,
                x => x.Total);

        //----------------------------------------------------
        // Peringkat (kelas tanpa kehadiran hari ini tidak ditampilkan)
        //----------------------------------------------------

        var ranked = classes
            .Select(x =>
            {
                var attendedCount = attended.GetValueOrDefault(
                    x.ClassRoomId);

                return new BestClassDto
                {
                    ClassRoomId = x.ClassRoomId,

                    ClassRoomName = x.ClassRoomName,

                    AttendedCount = attendedCount,

                    TotalStudents = x.TotalStudents,

                    AttendanceRate = Percent(
                        attendedCount,
                        x.TotalStudents)
                };
            })
            .Where(x => x.AttendedCount > 0)
            .OrderByDescending(x => x.AttendanceRate)
            .ThenByDescending(x => x.AttendedCount)
            .ThenBy(x => x.ClassRoomName)
            .Take(limit)
            .ToList();

        for (var i = 0; i < ranked.Count; i++)
        {
            ranked[i].Rank = i + 1;
        }

        return ranked;
    }

    public async Task<PagedStudentMonitoringResponse>
        GetStudentMonitoringAsync(StudentMonitoringFilterRequest request)
    {
        var pageNumber = Math.Max(request.PageNumber, 1);

        var pageSize = Math.Clamp(
            request.PageSize,
            1,
            MaxMonitoringPageSize);

        var today = DateOnly.FromDateTime(
            _dateTimeProvider.UtcNow);

        // Presensi dan KAIH dihitung untuk bulan berjalan.
        var monthStart = new DateOnly(
            today.Year,
            today.Month,
            1);

        //----------------------------------------------------
        // Daftar siswa (satu halaman)
        //----------------------------------------------------

        var query = _context.Students
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.NIS.ToLower().Contains(keyword) ||
                (x.NISN != null && x.NISN.ToLower().Contains(keyword)) ||
                x.FullName.ToLower().Contains(keyword));
        }

        if (request.ClassRoomId.HasValue)
        {
            query = query.Where(x =>
                x.ClassRoomId == request.ClassRoomId.Value);
        }

        var total = await query.CountAsync();

        var students = await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,

                x.NIS,

                x.FullName,

                x.ClassRoomId,

                ClassRoomName = x.ClassRoom.Name
            })
            .ToListAsync();

        var response = new PagedStudentMonitoringResponse
        {
            TotalRecords = total,

            PageNumber = pageNumber,

            PageSize = pageSize,

            TotalPages = (int)Math.Ceiling(
                total / (double)pageSize)
        };

        if (students.Count == 0)
            return response;

        var studentIds = students
            .Select(x => x.Id)
            .ToList();

        //----------------------------------------------------
        // Hari efektif bulan ini = tanggal yang punya data presensi
        //----------------------------------------------------

        var effectiveDays = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                x.AttendanceDate >= monthStart &&
                x.AttendanceDate <= today)
            .Select(x => x.AttendanceDate)
            .Distinct()
            .CountAsync();

        //----------------------------------------------------
        // Hari hadir (Hadir + Terlambat) per siswa
        //----------------------------------------------------

        var attendedDays = await _context.Attendances
            .Where(x =>
                !x.IsDeleted &&
                studentIds.Contains(x.StudentId) &&
                x.AttendanceDate >= monthStart &&
                x.AttendanceDate <= today &&
                (x.Status == AttendanceStatus.Present ||
                 x.Status == AttendanceStatus.Late))
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,

                Total = g.Count()
            })
            .ToDictionaryAsync(
                x => x.StudentId,
                x => x.Total);

        //----------------------------------------------------
        // Akumulasi poin pelanggaran per siswa
        // (poin positif dikurangi koreksi negatif, minimal 0)
        //----------------------------------------------------

        var violationPoints = await _context.AttendancePoints
            .Where(x =>
                !x.IsDeleted &&
                studentIds.Contains(x.StudentId))
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,

                Total = g.Sum(x => x.Point)
            })
            .ToDictionaryAsync(
                x => x.StudentId,
                x => x.Total);

        //----------------------------------------------------
        // KAIH bulan ini per siswa
        //----------------------------------------------------

        var kaih = await _context.KaihEntries
            .Where(x =>
                !x.IsDeleted &&
                studentIds.Contains(x.StudentId) &&
                x.EntryDate >= monthStart &&
                x.EntryDate <= today)
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

        //----------------------------------------------------
        // Response
        //----------------------------------------------------

        response.Items = students
            .Select(x =>
            {
                var points = Math.Max(
                    violationPoints.GetValueOrDefault(x.Id),
                    0);

                var status = StudentMonitoringHelper.GetStatus(points);

                var kaihDays = 0;

                var kaihRate = 0m;

                if (kaih.TryGetValue(x.Id, out var kaihRow))
                {
                    kaihDays = kaihRow.Days;

                    kaihRate = Percent(
                        kaihRow.Habits,
                        kaihRow.Days * KaihHabitCount);
                }

                return new StudentMonitoringItemDto
                {
                    StudentId = x.Id,

                    NIS = x.NIS,

                    FullName = x.FullName,

                    ClassRoomId = x.ClassRoomId,

                    ClassRoomName = x.ClassRoomName,

                    AttendanceRate = Percent(
                        attendedDays.GetValueOrDefault(x.Id),
                        effectiveDays),

                    ViolationPoints = points,

                    KaihRate = kaihRate,

                    KaihSubmittedDays = kaihDays,

                    Status = status.Key,

                    StatusLabel = status.Label
                };
            })
            .ToList();

        return response;
    }
}
