using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Features.Authentication.Login;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        SmartSchoolDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request)
    {
        // 1. Validate ClientType
        if (!Enum.IsDefined(typeof(ClientType), request.ClientType))
        {
            throw new UnauthorizedException(
                "Client type tidak valid.");
        }

        // 2. Find user
        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.Username == request.Username &&
                !x.IsDeleted);

        // Jangan membedakan username tidak ditemukan
        // dengan password salah.
        if (user == null)
        {
            throw new UnauthorizedException(
                "Username atau password salah.");
        }

        // 3. Verify password
        if (!_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedException(
                "Username atau password salah.");
        }

        // 4. Check user active
        if (!user.IsActive)
        {
            throw new UnauthorizedException(
                "User tidak aktif.");
        }

        // 5. Check role
        if (user.Role == null ||
            string.IsNullOrWhiteSpace(user.Role.Name))
        {
            throw new UnauthorizedException(
                "Role user tidak valid.");
        }

        // 6. Check role active
        if (!user.Role.IsActive)
        {
            throw new UnauthorizedException(
                "Role user tidak aktif.");
        }

        var role = user.Role.Name
            .Trim()
            .ToUpperInvariant();

        // 7. Validate role
        if (role != "ADMIN" &&
            role != "SCAN_OFFICER" &&
            role != "GUARDIAN")
        {
            throw new UnauthorizedException(
                "Role user tidak valid.");
        }

        // 8. Validate ClientType
        ValidateClientType(
            role,
            request.ClientType);

        // 9. Update LastLogin
        user.LastLogin = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 10. Generate JWT
        var jwt = _jwtTokenGenerator.GenerateToken(
            user,
            request.ClientType);

        // 11. Return response
        return new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.Name,
            ClientType = request.ClientType.ToString(),
            Token = jwt.Token,
            ExpiresAt = jwt.ExpiresAt
        };
    }

    private static void ValidateClientType(
        string role,
        ClientType clientType)
    {
        switch (role)
        {
            case "GUARDIAN":

                if (clientType != ClientType.MOBILE)
                {
                    throw new UnauthorizedException(
                        "Wali murid hanya dapat login melalui aplikasi mobile.");
                }

                break;

            case "ADMIN":
            case "SCAN_OFFICER":

                if (clientType != ClientType.WEB)
                {
                    throw new UnauthorizedException(
                        "User ini hanya dapat login melalui aplikasi web.");
                }

                break;

            default:

                throw new UnauthorizedException(
                    "Role user tidak valid.");
        }
    }
}