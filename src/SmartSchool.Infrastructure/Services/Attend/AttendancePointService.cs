using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Features.ActivityLogs;
using SmartSchool.Application.Features.ActivityLogs.Interfaces;
using SmartSchool.Application.Features.AttendancePoints.Contracts;
using SmartSchool.Application.Features.AttendancePoints.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Attend;

public class AttendancePointService : IAttendancePointService
{
    private readonly SmartSchoolDbContext _context;

    private readonly IActivityLogService _activityLog;

    public AttendancePointService(
        SmartSchoolDbContext context,
        IActivityLogService activityLog)
    {
        _context = context;

        _activityLog = activityLog;
    }

    public async Task<PagedAttendancePointResponse> GetPagedAsync(
        AttendancePointFilterRequest request)
    {
        var query = _context.AttendancePoints
            .Include(x => x.Attendance)
                .ThenInclude(x => x.Student)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.StudentId.HasValue)
        {
            query = query.Where(x =>
                x.Attendance.StudentId == request.StudentId);
        }

        if (request.AttendanceId.HasValue)
        {
            query = query.Where(x =>
                x.AttendanceId == request.AttendanceId);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedAttendancePointResponse
        {
            Items = items
                .Select(AttendancePointDto.FromEntity)
                .ToList(),

            TotalRecords = totalRecords,

            PageNumber = request.PageNumber,

            PageSize = request.PageSize,

            TotalPages = (int)Math.Ceiling(
                totalRecords / (double)request.PageSize)
        };
    }

    public async Task<AttendancePointDto> GetByIdAsync(Guid id)
    {
        var entity = await _context.AttendancePoints
            .Include(x => x.Attendance)
                .ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Attendance Point not found.");

        return AttendancePointDto.FromEntity(entity);
    }

    public async Task<Guid> CreateAsync(
        CreateAttendancePointRequest request)
    {
        var attendanceExists = await _context.Attendances
            .AnyAsync(x =>
                x.Id == request.AttendanceId &&
                !x.IsDeleted);
                 var attendance = await _context.Attendances
        .Include(x => x.Student)
        .FirstOrDefaultAsync(x =>
            x.Id == request.AttendanceId &&
            !x.IsDeleted);

        // if (!attendanceExists)
        //     throw new Exception("Attendance not found.");
        if (attendance == null)
    throw new Exception("Attendance not found.");

        var entity = new AttendancePoint
        {
            // Id = Guid.NewGuid(),

            // AttendanceId = request.AttendanceId,

            // Point = request.Point
             Id = Guid.NewGuid(),

        StudentId = attendance.StudentId,
        Reason = request.Reason,

        AttendanceId = attendance.Id,

        Point = request.Point,

        Description = request.Description,

        PointDate = DateTime.UtcNow
        };

        var isViolation = request.Point < 0;

        _activityLog.Record(
            isViolation
                ? ActivityLogTypes.ViolationAdded
                : ActivityLogTypes.PointAdded,
            (isViolation
                ? "Poin pelanggaran ditambahkan untuk "
                : "Poin ditambahkan untuk ") + attendance.Student.FullName,
            request.Reason + " (" + request.Point + ")",
            entity.Id);

        _context.AttendancePoints.Add(entity);

        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateAttendancePointRequest request)
    {
        var entity = await _context.AttendancePoints
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Attendance Point not found.");

         entity.Point = request.Point;

    entity.Reason = request.Reason;

    entity.Description = request.Description;

    entity.PointDate = DateTime.UtcNow;

    entity.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.AttendancePoints
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Attendance Point not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    public async Task<LeaderboardResponse> GetLeaderboardAsync(
    LeaderboardRequest request)
{
    var query = _context.AttendancePoints
        .Include(x => x.Student)
            .ThenInclude(x => x.ClassRoom)
        .Where(x => !x.IsDeleted);

    if (request.ClassRoomId.HasValue)
    {
        query = query.Where(x =>
            x.Student.ClassRoomId == request.ClassRoomId);
    }

    if (request.StartDate.HasValue)
    {
        var start = request.StartDate.Value.ToDateTime(TimeOnly.MinValue);

        query = query.Where(x =>
            x.PointDate >= start);
    }

    if (request.EndDate.HasValue)
    {
        var end = request.EndDate.Value.ToDateTime(TimeOnly.MaxValue);

        query = query.Where(x =>
            x.PointDate <= end);
    }

    var ranking = await query
        .GroupBy(x => new
        {
            x.StudentId,
            x.Student.NIS,
            x.Student.FullName,
            ClassRoom = x.Student.ClassRoom.Name
        })
        .Select(x => new
        {
            x.Key.StudentId,
            x.Key.NIS,
            x.Key.FullName,
            x.Key.ClassRoom,
            TotalPoint = x.Sum(y => y.Point)
        })
        .OrderByDescending(x => x.TotalPoint)
        .Take(request.Top)
        .ToListAsync();

    var response = new LeaderboardResponse();

    var rank = 1;

    foreach (var item in ranking)
    {
        response.Items.Add(new LeaderboardDto
        {
            Rank = rank++,
            StudentId = item.StudentId,
            NIS = item.NIS,
            StudentName = item.FullName,
            ClassRoomName = item.ClassRoom,
            TotalPoint = item.TotalPoint
        });
    }

    return response;
}
}