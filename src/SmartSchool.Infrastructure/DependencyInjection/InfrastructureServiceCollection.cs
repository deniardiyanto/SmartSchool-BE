using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartSchool.Infrastructure.Persistence.Context;
using SmartSchool.Application.Features.ClassRooms.Interfaces;
using SmartSchool.Infrastructure.Services.Master;
using SmartSchool.Application.Features.Guardians.Interfaces;

using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Common.Settings;

using SmartSchool.Infrastructure.Security;
using SmartSchool.Infrastructure.Services.Authentication;
using SmartSchool.Application.Features.Authentication.Login;
using SmartSchool.Infrastructure.Services;
using SmartSchool.Application.Features.Students.Interfaces;
using SmartSchool.Application.Features.BarcodeCards.Interfaces;
using SmartSchool.Application.Features.Attendances.Interfaces;
using SmartSchool.Infrastructure.Services.Attend;
using SmartSchool.Application.Features.Attendances.Scan.Interfaces;
using SmartSchool.Application.Features.AttendancePoints.Interfaces;
using SmartSchool.Application.Features.Attendances.Dashboard.Interfaces;
using SmartSchool.Application.Features.WhatsApp.Interfaces;
using SmartSchool.Infrastructure.Services.WhatsApp;
using SmartSchool.Infrastructure.Configuration;
using SmartSchool.Application.Features.Dashboard.Interfaces;
using SmartSchool.Infrastructure.Services.Dashboard;
using SmartSchool.Application.Features.Master.ClassRooms.Import.Interfaces;
using SmartSchool.Infrastructure.Services.Master.ClassRooms;
using SmartSchool.Application.Features.Master.Guardians.Import.Interfaces;
using SmartSchool.Infrastructure.Services.Master.Guardians;
using SmartSchool.Application.Features.Master.Students.Import.Interfaces;
using SmartSchool.Infrastructure.Services.Master.Students;

using Microsoft.Extensions.Options;


namespace SmartSchool.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<SmartSchoolDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClassRoomService, ClassRoomService>();
        services.AddScoped<IGuardianService, GuardianService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IBarcodeCardService, BarcodeCardService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAttendanceScannerService, AttendanceScannerService>();
        services.AddScoped<IAttendancePointService, AttendancePointService>();
        services.AddScoped<
        IAttendanceDashboardService,
        AttendanceDashboardService>();
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();
        services.Configure<FonnteOptions>(
            configuration.GetSection(FonnteOptions.SectionName));
        services.Configure<WablasOptions>(
            configuration.GetSection(WablasOptions.SectionName));
        services.Configure<SchoolOptions>(
        configuration.GetSection(SchoolOptions.SectionName));
        services.AddScoped<
        IAttendanceMessageBuilder,
        AttendanceMessageBuilder>();
        services.AddScoped<IWhatsAppLogService,
        WhatsAppLogService>();
        // services.AddScoped<IWhatsAppService, FonnteWhatsAppService>();

        services.AddScoped<IWhatsAppLogService, WhatsAppLogService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IClassRoomImportService, ClassRoomImportService>();
        services.AddScoped<IGuardianImportService, GuardianImportService>();
        services.AddScoped<IStudentImportService, StudentImportService>();
        services.AddHttpClient<IWhatsAppService, WablasWhatsAppService>(
    (provider, client) =>
    {
        var options = provider
            .GetRequiredService<IOptions<WablasOptions>>()
            .Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(30);

        client.DefaultRequestHeaders.Clear();

        client.DefaultRequestHeaders.Add(
            "Authorization",
            $"{options.Token}.{options.SecretKey}");

        client.DefaultRequestHeaders.Add(
            "Accept",
            "application/json");
    });
        return services;
    }

}