using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Common.Settings;
using SmartSchool.Application.Features.Authentication.Login;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        SmartSchoolDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtOptions.Value;
    }

    // public async Task<LoginResponse> LoginAsync(LoginRequest request)
    // {
    //     var user = await _context.Users
    //         .Include(x => x.Role)
    //         .FirstOrDefaultAsync(x =>
    //             x.Username == request.Username &&
    //             !x.IsDeleted);

    //     if (user == null)
    //         throw new UnauthorizedException("Username atau password salah.");

    //     if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
    //         throw new UnauthorizedException("Username atau password salah.");

    //     if (!user.IsActive)
    //         throw new UnauthorizedException("User tidak aktif.");

    //     var token = _jwtTokenGenerator.GenerateToken(user);

    //     return new LoginResponse
    //     {
    //         UserId = user.Id,
    //         Username = user.Username,
    //         FullName = user.FullName,
    //         Role = user.Role.Name,
    //         Token = token,
    //         ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes)
    //     };
    // }
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.Username == request.Username &&
                !x.IsDeleted);

        if (user == null)
            throw new UnauthorizedException("Username atau password salah.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Username atau password salah.");

        if (!user.IsActive)
            throw new UnauthorizedException("User tidak aktif.");

        var role = user.Role.Name.ToUpperInvariant();

        if (role == "GUARDIAN" &&
            request.ClientType != ClientType.MOBILE)
        {
            throw new UnauthorizedException(
                "Wali murid hanya dapat login melalui aplikasi mobile.");
        }

        if ((role == "ADMIN" || role == "SCAN_OFFICER") &&
            request.ClientType != ClientType.WEB)
        {
            throw new UnauthorizedException(
                "User ini hanya dapat login melalui aplikasi web.");
        }

        if (role != "ADMIN" &&
            role != "SCAN_OFFICER" &&
            role != "GUARDIAN")
        {
            throw new UnauthorizedException(
                "Role user tidak valid.");
        }

        var token = _jwtTokenGenerator.GenerateToken(
            user,
            request.ClientType);

        return new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.Name,
            ClientType = request.ClientType.ToString(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpireMinutes)
        };
    }
}