using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Common.Models;
using SmartSchool.Application.Features.Attendances.Scan.Contracts;
using SmartSchool.Application.Features.Attendances.Scan.Interfaces;
using SmartSchool.Application.Features.WhatsApp.Contracts;
using SmartSchool.Application.Features.WhatsApp.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Domain.Enums;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Attend;

public class AttendanceScannerService : IAttendanceScannerService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IAttendanceMessageBuilder _messageBuilder;

    public AttendanceScannerService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUser,
        IWhatsAppService whatsAppService,
        IAttendanceMessageBuilder messageBuilder)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
        _whatsAppService = whatsAppService;
        _messageBuilder = messageBuilder;
    }

    public async Task<ApiResponse<ScanAttendanceResponse>> ScanAsync(
        ScanAttendanceRequest request)
    {
        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var rule = await GetAttendanceRuleAsync();

        //-------------------------------------------------------
        // Cari barcode
        //-------------------------------------------------------

        var barcode = await _context.BarcodeCards
            .Include(x => x.Student)
                .ThenInclude(x => x.ClassRoom)
            .Include(x => x.Student)
                .ThenInclude(x => x.Guardian)
            .FirstOrDefaultAsync(x =>
                x.BarcodeValue == request.BarcodeValue &&
                !x.IsDeleted);

        if (barcode == null)
        {
            return ApiResponse<ScanAttendanceResponse>.Fail(
                "Barcode tidak ditemukan.");
        }

        if (!barcode.IsActive)
        {
            return ApiResponse<ScanAttendanceResponse>.Fail(
                "Barcode sudah tidak aktif.");
        }

        if (barcode.ExpiredDate.HasValue &&
            barcode.ExpiredDate.Value < now)
        {
            return ApiResponse<ScanAttendanceResponse>.Fail(
                "Barcode sudah expired.");
        }

        var student = barcode.Student;

        if (!student.IsActive)
        {
            return ApiResponse<ScanAttendanceResponse>.Fail(
                "Student sudah tidak aktif.");
        }

        //-------------------------------------------------------
        // Attendance hari ini
        //-------------------------------------------------------

        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(x =>
                x.StudentId == student.Id &&
                x.AttendanceDate == today &&
                !x.IsDeleted);

        //-------------------------------------------------------
        // CHECK IN
        //-------------------------------------------------------

        if (attendance == null)
        {
            return await ProcessCheckInAsync(
                student,
                barcode,
                rule,
                now,
                today);
        }

        //-------------------------------------------------------
        // CHECK OUT
        //-------------------------------------------------------

        if (attendance.CheckOutTime == null)
        {
            return await ProcessCheckOutAsync(
                student,
                barcode,
                attendance,
                now);
        }

        //-------------------------------------------------------
        // SUDAH CHECK OUT
        //-------------------------------------------------------

        return ApiResponse<ScanAttendanceResponse>.Fail(
            "Attendance hari ini sudah selesai.");
    }

    /// <summary>
    /// Proses Check-In
    /// </summary>
    private async Task<ApiResponse<ScanAttendanceResponse>>
        ProcessCheckInAsync(
        Student student,
        BarcodeCard barcode,
        AttendanceRule rule,
        DateTime now,
        DateOnly today)
    {
        var result = CalculateAttendanceResult(
            now,
            rule);

        var attendance = await CreateAttendanceAsync(
            student,
            barcode,
            result.status,
            now,
            today);

        await CreateAttendancePointAsync(
            attendance,
            rule);

//disable WA notification for now 
    //     try
    //     {
    //         await SendAttendanceNotificationAsync(
    //  student,
    //  attendance,
    //  result.point);
    //     }
    //     catch
    //     {
    //         // Jangan menggagalkan proses scan
    //     }

        return ApiResponse<ScanAttendanceResponse>.Ok(
            BuildCheckInResponse(
                attendance,
                student,
                barcode,
                result,
                now),
            "Check-in berhasil.");
    }
    /// <summary>
    /// Proses Check-Out
    /// </summary>
    private async Task<ApiResponse<ScanAttendanceResponse>>
        ProcessCheckOutAsync(
        Student student,
        BarcodeCard barcode,
        Attendance attendance,
        DateTime now)
    {
        attendance.CheckOutTime = now;
        attendance.UpdatedAt = now;
        attendance.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync();

//diable WA notification for now
    //     try
    //     {
    //         await SendAttendanceNotificationAsync(
    // student,
    // attendance,
    // 0);
    //     }
    //     catch
    //     {
    //         // Ignore jika WA gagal
    //     }

        return ApiResponse<ScanAttendanceResponse>.Ok(
            BuildCheckOutResponse(
                attendance,
                student,
                barcode,
                now),
            "Check-out berhasil.");
    }

    /// <summary>
    /// Membuat Attendance baru saat Check-In
    /// </summary>
    private async Task<Attendance> CreateAttendanceAsync(
        Student student,
        BarcodeCard barcode,
        AttendanceStatus status,
        DateTime now,
        DateOnly today)
    {
        var attendance = new Attendance
        {
            Id = Guid.NewGuid(),

            StudentId = student.Id,

            BarcodeCardId = barcode.Id,

            AttendanceDate = today,

            CheckInTime = now,

            Status = status,

            CreatedAt = now,

            CreatedBy = _currentUser.UserId
        };

        _context.Attendances.Add(attendance);

        await _context.SaveChangesAsync();

        return attendance;
    }

    /// <summary>
    /// Response Check-In
    /// </summary>
    private static ScanAttendanceResponse BuildCheckInResponse(
        Attendance attendance,
        Student student,
        BarcodeCard barcode,
        (AttendanceStatus status, int point, string reason) result,
        DateTime now)
    {
        return new ScanAttendanceResponse
        {
            AttendanceId = attendance.Id,

            StudentId = student.Id,

            StudentName = student.FullName,

            ClassRoomName = student.ClassRoom.Name,

            BarcodeValue = barcode.BarcodeValue,

            ScanType = "CheckIn",

            ScanTime = now,

            Status = attendance.Status.ToString(),

            Point = result.point,

            Reason = result.reason
        };
    }

    /// <summary>
    /// Response Check-Out
    /// </summary>
    private static ScanAttendanceResponse BuildCheckOutResponse(
        Attendance attendance,
        Student student,
        BarcodeCard barcode,
        DateTime now)
    {
        return new ScanAttendanceResponse
        {
            AttendanceId = attendance.Id,

            StudentId = student.Id,

            StudentName = student.FullName,

            ClassRoomName = student.ClassRoom.Name,

            BarcodeValue = barcode.BarcodeValue,

            ScanType = "CheckOut",

            ScanTime = now,

            Status = attendance.Status.ToString()
        };
    }
    /// <summary>
    /// Ambil Attendance Rule aktif
    /// </summary>
    private async Task<AttendanceRule> GetAttendanceRuleAsync()
    {
        var rule = await _context.AttendanceRules
            .FirstOrDefaultAsync(x =>
                x.IsActive &&
                !x.IsDeleted);

        if (rule == null)
            throw new Exception(
                "Attendance Rule belum dikonfigurasi.");

        return rule;
    }

    /// <summary>
    /// Menghitung status dan point berdasarkan rule.
    /// </summary>
    private (AttendanceStatus status, int point, string reason)
        CalculateAttendanceResult(
        DateTime now,
        AttendanceRule rule)
    {
        var currentTime = TimeOnly.FromDateTime(now);

        if (currentTime <= rule.CheckInEnd)
        {
            return
            (
                AttendanceStatus.Present,
                rule.PresentPoint,
                "Present"
            );
        }

        return
        (
            AttendanceStatus.Late,
            rule.LatePoint,
            "Late"
        );
    }

    /// <summary>
    /// Membuat Attendance Point otomatis.
    /// </summary>
    private async Task CreateAttendancePointAsync(
        Attendance attendance,
        AttendanceRule rule)
    {
        var exists = await _context.AttendancePoints
            .AnyAsync(x =>
                x.AttendanceId == attendance.Id &&
                !x.IsDeleted);

        if (exists)
            return;

        int point;
        string reason;
        string description;

        switch (attendance.Status)
        {
            case AttendanceStatus.Present:
                point = rule.PresentPoint;
                reason = "Present";
                description = "Student checked in on time.";
                break;

            case AttendanceStatus.Late:
                point = rule.LatePoint;
                reason = "Late";
                description = "Student checked in after allowed time.";
                break;

            default:
                point = 0;
                reason = attendance.Status.ToString();
                description = "Attendance point generated automatically.";
                break;
        }

        var entity = new AttendancePoint
        {
            Id = Guid.NewGuid(),

            AttendanceId = attendance.Id,

            StudentId = attendance.StudentId,

            Point = point,

            Reason = reason,

            Description = description,

            PointDate = _dateTimeProvider.UtcNow,

            CreatedAt = _dateTimeProvider.UtcNow,

            CreatedBy = _currentUser.UserId
        };

        _context.AttendancePoints.Add(entity);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Kirim notifikasi WhatsApp ke Guardian.
    /// </summary>
    //    private async Task SendAttendanceNotificationAsync(
    //     Student student,
    //     Attendance attendance,
    //     int point)
    //     {
    //         if (student.Guardian == null)
    //             return;

    //         if (string.IsNullOrWhiteSpace(
    //             student.Guardian.PhoneNumber))
    //             return;

    //         string message;

    //         if (attendance.CheckOutTime == null)
    //         {
    //             message = _messageBuilder.BuildCheckInMessage(
    //                 student,
    //                 attendance);
    //         }
    //         else
    //         {
    //             message = _messageBuilder.BuildCheckOutMessage(
    //                 student,
    //                 attendance);
    //         }

    //         var response =
    //             await _whatsAppService.SendAsync(
    //                 new SendWhatsAppRequest
    //                 {
    //                     PhoneNumber = student.Guardian.PhoneNumber,
    //                     Message = message
    //                 });

    //         await SaveWhatsAppLogAsync(
    //             attendance,
    //             student.Guardian.PhoneNumber,
    //             message,
    //             response);
    //     }
    private async Task SendAttendanceNotificationAsync(
        Student student,
        Attendance attendance,
        int point)
    {
        if (student.Guardian == null)
            return;

        if (string.IsNullOrWhiteSpace(student.Guardian.PhoneNumber))
            return;

        string message;

        if (attendance.CheckOutTime == null)
        {
            message = _messageBuilder.BuildCheckInMessage(
                student,
                attendance,
                point);
        }
        else
        {
            message = _messageBuilder.BuildCheckOutMessage(
                student,
                attendance);
        }

        var response =
            await _whatsAppService.SendAsync(
                new SendWhatsAppRequest
                {
                    PhoneNumber = student.Guardian.PhoneNumber,
                    Message = message
                });

        await SaveWhatsAppLogAsync(
            attendance,
            student.Guardian.PhoneNumber,
            message,
            response);
    }
    /// <summary>
    /// Simpan log WhatsApp.
    /// </summary>
    private async Task SaveWhatsAppLogAsync(
        Attendance attendance,
        string phoneNumber,
        string message,
        SendWhatsAppResponse response)
    {
        var entity = new WhatsAppLog
        {
            Id = Guid.NewGuid(),

            AttendanceId = attendance.Id,

            PhoneNumber = phoneNumber,

            Message = message,

            Status = response.Success
                ? "Success"
                : "Failed",

            ProviderResponse = response.ProviderMessage,

            SentAt = _dateTimeProvider.UtcNow,

            CreatedAt = _dateTimeProvider.UtcNow,

            CreatedBy = _currentUser.UserId
        };

        _context.WhatsAppLogs.Add(entity);

        await _context.SaveChangesAsync();
    }
}