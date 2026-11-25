using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Features.Attendances.CloseDay.Contracts;
using SmartSchool.Application.Features.Attendances.CloseDay.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Domain.Enums;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Attend;

public class AttendanceCloseDayService : IAttendanceCloseDayService
{
    // Jam pada aturan presensi diisi dalam WIB, 'now' berupa UTC.
    private const int WibOffsetHours = 7;

    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUser;

    public AttendanceCloseDayService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public async Task<CloseDayResponse> CloseDayAsync(
        CloseDayRequest request)
    {
        var now = _dateTimeProvider.UtcNow;

        var today = DateOnly.FromDateTime(now);

        var date = request.Date ?? today;

        if (date > today)
            throw new BadRequestException(
                "Tanggal tidak boleh di masa depan.");

        //----------------------------------------------------
        // Aturan presensi aktif
        //----------------------------------------------------

        var rule = await _context.AttendanceRules
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (rule == null)
            throw new BadRequestException(
                "Aturan presensi belum dikonfigurasi.");

        // Hari ini hanya boleh ditutup setelah jam pulang dibuka,
        // agar siswa yang belum sempat scan tidak langsung Alpa.
        if (date == today)
        {
            var nowWib = TimeOnly.FromDateTime(
                now.AddHours(WibOffsetHours));

            if (nowWib < rule.CheckOutStart)
                throw new BadRequestException(
                    $"Tutup hari ini baru bisa dilakukan setelah pukul {rule.CheckOutStart:HH\\:mm} WIB.");
        }

        //----------------------------------------------------
        // Hari efektif: hanya tanggal yang sudah punya presensi
        //----------------------------------------------------

        var hasAttendance = await _context.Attendances
            .AnyAsync(x =>
                !x.IsDeleted &&
                x.AttendanceDate == date);

        if (!hasAttendance)
            throw new BadRequestException(
                "Belum ada presensi pada tanggal ini, sehingga tidak dianggap hari efektif.");

        //----------------------------------------------------
        // Siswa aktif yang belum punya baris presensi.
        // Baris yang sudah dihapus (soft delete) ikut dihitung karena
        // indeks unik (siswa, tanggal) tetap berlaku untuknya.
        //----------------------------------------------------

        var recordedIds = (await _context.Attendances
            .Where(x => x.AttendanceDate == date)
            .Select(x => x.StudentId)
            .ToListAsync())
            .ToHashSet();

        var activeIds = await _context.Students
            .Where(x => x.IsActive && !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync();

        var missing = activeIds
            .Where(id => !recordedIds.Contains(id))
            .ToList();

        var pointDate = date == today
            ? now
            : date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        foreach (var studentId in missing)
        {
            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                AttendanceDate = date,
                Status = AttendanceStatus.Absent,
                Notes = "Ditandai alpa otomatis (tutup hari).",
                CreatedAt = now,
                CreatedBy = _currentUser.UserId
            };

            _context.Attendances.Add(attendance);

            _context.AttendancePoints.Add(new AttendancePoint
            {
                Id = Guid.NewGuid(),
                AttendanceId = attendance.Id,
                StudentId = studentId,
                Point = rule.AbsentPoint,
                Reason = "Absent",
                Description = "Siswa tidak melakukan presensi (tutup hari).",
                PointDate = pointDate,
                CreatedAt = now,
                CreatedBy = _currentUser.UserId
            });
        }

        if (missing.Count > 0)
            await _context.SaveChangesAsync();

        return new CloseDayResponse
        {
            Date = date,
            MarkedAbsent = missing.Count,
            AlreadyRecorded = activeIds.Count - missing.Count,
            TotalActiveStudents = activeIds.Count,
            AbsentPoint = rule.AbsentPoint
        };
    }
}
