using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Features.ActivityLogs;
using SmartSchool.Application.Features.ActivityLogs.Interfaces;
using SmartSchool.Application.Features.Attendances.Contracts;
using SmartSchool.Application.Features.Attendances.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Attend;

public class AttendanceService : IAttendanceService
{
    private readonly SmartSchoolDbContext _context;

    private readonly IActivityLogService _activityLog;

    public AttendanceService(
        SmartSchoolDbContext context,
        IActivityLogService activityLog)
    {
        _context = context;

        _activityLog = activityLog;
    }

    public async Task<PagedAttendanceResponse> GetPagedAsync(
        AttendanceFilterRequest request)
    {
        var query = _context.Attendances
            .Include(x => x.Student)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.StudentId.HasValue)
        {
            query = query.Where(x =>
                x.StudentId == request.StudentId.Value);
        }

        if (request.Date.HasValue)
        {
            query = query.Where(x =>
                x.AttendanceDate == request.Date.Value);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.AttendanceDate)
            .ThenBy(x => x.Student.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedAttendanceResponse
        {
            Items = items
                .Select(AttendanceDto.FromEntity)
                .ToList(),

            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                totalRecords / (double)request.PageSize)
        };
    }

    public async Task<AttendanceDto> GetByIdAsync(Guid id)
    {
        var entity = await _context.Attendances
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Attendance not found.");

        return AttendanceDto.FromEntity(entity);
    }

    public async Task<Guid> CreateAsync(
        CreateAttendanceRequest request)
    {
        var studentExists = await _context.Students
            .AnyAsync(x =>
                x.Id == request.StudentId &&
                !x.IsDeleted);

        if (!studentExists)
            throw new Exception("Student not found.");

        var attendanceExists = await _context.Attendances
            .AnyAsync(x =>
                x.StudentId == request.StudentId &&
                x.AttendanceDate == request.AttendanceDate &&
                !x.IsDeleted);

        if (attendanceExists)
            throw new Exception(
                "Attendance already exists for this student.");

        if (request.BarcodeCardId.HasValue)
        {
            var barcodeExists = await _context.BarcodeCards
                .AnyAsync(x =>
                    x.Id == request.BarcodeCardId.Value &&
                    !x.IsDeleted);

            if (!barcodeExists)
                throw new Exception("Barcode card not found.");
        }

        var entity = new Attendance
        {
            Id = Guid.NewGuid(),

            StudentId = request.StudentId,

            AttendanceDate = request.AttendanceDate,

            CheckInTime = request.CheckInTime.HasValue
                ? DateTime.SpecifyKind(
                    request.CheckInTime.Value,
                    DateTimeKind.Utc)
                : null,

            CheckOutTime = request.CheckOutTime.HasValue
                ? DateTime.SpecifyKind(
                    request.CheckOutTime.Value,
                    DateTimeKind.Utc)
                : null,

            Status = request.Status,

            Notes = request.Notes,

            BarcodeCardId = request.BarcodeCardId
        };

        var studentName = await _context.Students
            .Where(x => x.Id == request.StudentId)
            .Select(x => x.FullName)
            .FirstAsync();

        _activityLog.Record(
            ActivityLogTypes.AttendanceCreated,
            "Data presensi ditambahkan",
            studentName + " - " + request.Status,
            entity.Id);

        _context.Attendances.Add(entity);

        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateAttendanceRequest request)
    {
        var entity = await _context.Attendances
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Attendance not found.");

        entity.CheckInTime = request.CheckInTime.HasValue
            ? DateTime.SpecifyKind(
                request.CheckInTime.Value,
                DateTimeKind.Utc)
            : null;

        entity.CheckOutTime = request.CheckOutTime.HasValue
            ? DateTime.SpecifyKind(
                request.CheckOutTime.Value,
                DateTimeKind.Utc)
            : null;

        entity.Status = request.Status;

        entity.Notes = request.Notes;

        var studentName = await _context.Students
            .Where(x => x.Id == entity.StudentId)
            .Select(x => x.FullName)
            .FirstAsync();

        _activityLog.Record(
            ActivityLogTypes.AttendanceUpdated,
            "Data presensi diperbarui",
            studentName + " - " + request.Status,
            entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Attendances
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Attendance not found.");

        entity.IsDeleted = true;

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}