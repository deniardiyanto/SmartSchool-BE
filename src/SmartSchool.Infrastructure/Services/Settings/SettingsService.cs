using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Constants;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Features.Settings.Contracts;
using SmartSchool.Application.Features.Settings.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Settings;

public class SettingsService : ISettingsService
{
    private const int MaxPoint = 1000;

    private readonly SmartSchoolDbContext _context;

    public SettingsService(SmartSchoolDbContext context)
    {
        _context = context;
    }

    //--------------------------------------------------------
    // Aturan presensi
    //--------------------------------------------------------

    public async Task<AttendanceRuleDto?> GetAttendanceRuleAsync()
    {
        var rule = await FindActiveRuleAsync(track: false);

        return rule == null ? null : ToDto(rule);
    }

    public async Task<AttendanceRuleDto> SaveAttendanceRuleAsync(
        SaveAttendanceRuleRequest request)
    {
        var name = request.RuleName?.Trim() ?? string.Empty;

        if (name.Length == 0 || name.Length > 100)
            throw new BadRequestException(
                "Nama aturan wajib diisi (maksimal 100 karakter).");

        if (request.LateToleranceMinutes < 0 ||
            request.LateToleranceMinutes > 180)
            throw new BadRequestException(
                "Toleransi terlambat harus 0-180 menit.");

        ValidatePoint(request.PresentPoint, "Poin hadir");
        ValidatePoint(request.LatePoint, "Poin terlambat");
        ValidatePoint(request.AbsentPoint, "Poin alpa");
        ValidatePoint(request.EarlyCheckoutPoint, "Poin pulang cepat");

        var rule = await FindActiveRuleAsync(track: true);

        if (rule == null)
        {
            rule = new AttendanceRule
            {
                Id = Guid.NewGuid(),
                IsActive = true
            };

            _context.AttendanceRules.Add(rule);
        }
        else
        {
            rule.UpdatedAt = DateTime.UtcNow;
        }

        rule.RuleName = name;
        rule.CheckInStart = request.CheckInStart;
        rule.CheckInEnd = request.CheckInEnd;
        rule.CheckOutStart = request.CheckOutStart;
        rule.CheckOutEnd = request.CheckOutEnd;
        rule.LateToleranceMinutes = request.LateToleranceMinutes;
        rule.PresentPoint = request.PresentPoint;
        rule.LatePoint = request.LatePoint;
        rule.AbsentPoint = request.AbsentPoint;
        rule.EarlyCheckoutPoint = request.EarlyCheckoutPoint;

        await _context.SaveChangesAsync();

        return ToDto(rule);
    }

    //--------------------------------------------------------
    // Ambang status siswa
    //--------------------------------------------------------

    public async Task<MonitoringSettingDto> GetMonitoringSettingAsync()
    {
        var setting = await _context.MonitoringSettings
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (setting == null)
        {
            return new MonitoringSettingDto
            {
                WarningPoint = StudentMonitoringConstant.WarningPoint,
                CoachingPoint = StudentMonitoringConstant.CoachingPoint,
                IsDefault = true
            };
        }

        return new MonitoringSettingDto
        {
            WarningPoint = setting.WarningPoint,
            CoachingPoint = setting.CoachingPoint,
            IsDefault = false
        };
    }

    public async Task<MonitoringSettingDto> SaveMonitoringSettingAsync(
        SaveMonitoringSettingRequest request)
    {
        if (request.WarningPoint < 1 || request.WarningPoint > MaxPoint)
            throw new BadRequestException(
                $"Ambang Peringatan harus 1-{MaxPoint}.");

        if (request.CoachingPoint > MaxPoint ||
            request.CoachingPoint <= request.WarningPoint)
            throw new BadRequestException(
                "Ambang Pembinaan harus lebih besar dari ambang Peringatan.");

        var setting = await _context.MonitoringSettings
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (setting == null)
        {
            setting = new MonitoringSetting
            {
                Id = Guid.NewGuid()
            };

            _context.MonitoringSettings.Add(setting);
        }
        else
        {
            setting.UpdatedAt = DateTime.UtcNow;
        }

        setting.WarningPoint = request.WarningPoint;
        setting.CoachingPoint = request.CoachingPoint;

        await _context.SaveChangesAsync();

        return new MonitoringSettingDto
        {
            WarningPoint = setting.WarningPoint,
            CoachingPoint = setting.CoachingPoint,
            IsDefault = false
        };
    }

    //--------------------------------------------------------
    // Helper
    //--------------------------------------------------------

    private Task<AttendanceRule?> FindActiveRuleAsync(bool track)
    {
        // Scanner memakai FirstOrDefault(IsActive && !IsDeleted);
        // urutan dibuat stabil agar baris yang sama yang diubah.
        var query = track
            ? _context.AttendanceRules.AsQueryable()
            : _context.AttendanceRules.AsNoTracking();

        return query
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private static void ValidatePoint(int value, string label)
    {
        if (value < -MaxPoint || value > MaxPoint)
            throw new BadRequestException(
                $"{label} harus antara -{MaxPoint} dan {MaxPoint}.");
    }

    private static AttendanceRuleDto ToDto(AttendanceRule x)
    {
        return new AttendanceRuleDto
        {
            Id = x.Id,
            RuleName = x.RuleName,
            CheckInStart = x.CheckInStart,
            CheckInEnd = x.CheckInEnd,
            CheckOutStart = x.CheckOutStart,
            CheckOutEnd = x.CheckOutEnd,
            LateToleranceMinutes = x.LateToleranceMinutes,
            PresentPoint = x.PresentPoint,
            LatePoint = x.LatePoint,
            AbsentPoint = x.AbsentPoint,
            EarlyCheckoutPoint = x.EarlyCheckoutPoint
        };
    }
}
