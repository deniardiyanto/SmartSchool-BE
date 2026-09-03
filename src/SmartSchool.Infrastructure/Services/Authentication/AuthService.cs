// using Microsoft.EntityFrameworkCore;
// using SmartSchool.Application.Common.Exceptions;
// using SmartSchool.Application.Common.Interfaces;
// using SmartSchool.Application.Features.Authentication.Login;
// using SmartSchool.Infrastructure.Persistence.Context;

// namespace SmartSchool.Infrastructure.Services.Authentication;

// public class AuthService : IAuthService
// {
//     private readonly SmartSchoolDbContext _context;
//     private readonly IPasswordHasher _passwordHasher;
//     private readonly IJwtTokenGenerator _jwtTokenGenerator;

//     public AuthService(
//         SmartSchoolDbContext context,
//         IPasswordHasher passwordHasher,
//         IJwtTokenGenerator jwtTokenGenerator)
//     {
//         _context = context;
//         _passwordHasher = passwordHasher;
//         _jwtTokenGenerator = jwtTokenGenerator;
//     }

//     public async Task<LoginResponse> LoginAsync(
//         LoginRequest request)
//     {
//         // =========================================================
//         // 1. VALIDATE CLIENT TYPE
//         // =========================================================

//         if (!Enum.IsDefined(typeof(ClientType), request.ClientType))
//         {
//             throw new UnauthorizedException(
//                 "Client type tidak valid.");
//         }

//         // =========================================================
//         // 2. LOGIN WEB
//         //    Hanya ADMIN yang boleh login melalui WEB
//         // =========================================================

//         if (request.ClientType == ClientType.WEB)
//         {
//             return await LoginWebAsync(request);
//         }

//         // =========================================================
//         // 3. LOGIN MOBILE
//         //    Login menggunakan NIS siswa
//         // =========================================================

//         return await LoginMobileAsync(request);
//     }

//     // =============================================================
//     // LOGIN WEB
//     // =============================================================

//     private async Task<LoginResponse> LoginWebAsync(
//         LoginRequest request)
//     {
//         // =========================================================
//         // Cari User berdasarkan Username
//         // =========================================================

//         var user = await _context.Users
//             .Include(x => x.Role)
//             .FirstOrDefaultAsync(x =>
//                 x.Username == request.Username &&
//                 !x.IsDeleted);

//         // =========================================================
//         // USER TIDAK DITEMUKAN
//         // =========================================================

//         if (user == null)
//         {
//             throw new UnauthorizedException(
//                 "Username atau password salah.");
//         }

//         // =========================================================
//         // CHECK USER ACTIVE
//         // =========================================================

//         if (!user.IsActive)
//         {
//             throw new UnauthorizedException(
//                 "User tidak aktif.");
//         }

//         if (user.IsDeleted)
//         {
//             throw new UnauthorizedException(
//                 "User tidak aktif.");
//         }

//         // =========================================================
//         // AMBIL ROLE
//         // =========================================================

//         var roleEntity = user.Role;

//         if (roleEntity == null)
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak valid.");
//         }

//         if (string.IsNullOrWhiteSpace(roleEntity.Name))
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak valid.");
//         }

//         // =========================================================
//         // CHECK ROLE ACTIVE
//         // =========================================================

//         if (!roleEntity.IsActive)
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak aktif.");
//         }

//         if (roleEntity.IsDeleted)
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak aktif.");
//         }

//         var role = roleEntity.Name
//             .Trim()
//             .ToUpperInvariant();

//         // =========================================================
//         // WEB HANYA ADMIN
//         // =========================================================

//         if (role != "ADMIN")
//         {
//             throw new UnauthorizedException(
//                 "Hanya Admin yang dapat login melalui aplikasi web.");
//         }

//         // =========================================================
//         // VERIFY PASSWORD
//         // =========================================================

//         if (!_passwordHasher.Verify(
//                 request.Password,
//                 user.PasswordHash))
//         {
//             throw new UnauthorizedException(
//                 "Username atau password salah.");
//         }

//         // =========================================================
//         // UPDATE LAST LOGIN
//         // =========================================================

//         user.LastLogin = DateTime.UtcNow;

//         await _context.SaveChangesAsync();

//         // =========================================================
//         // GENERATE JWT
//         // =========================================================

//         var jwt = _jwtTokenGenerator.GenerateToken(
//             user,
//             ClientType.WEB);

//         // =========================================================
//         // RESPONSE
//         // =========================================================

//         return new LoginResponse
//         {
//             UserId = user.Id,
//             Username = user.Username,
//             FullName = user.FullName,
//             Role = roleEntity.Name,
//             ClientType = ClientType.WEB.ToString(),
//             Token = jwt.Token,
//             ExpiresAt = jwt.ExpiresAt
//         };
//     }

//     // =============================================================
//     // LOGIN MOBILE
//     // =============================================================

//     private async Task<LoginResponse> LoginMobileAsync(
//         LoginRequest request)
//     {
//         // =========================================================
//         // Cari Student berdasarkan NIS
//         //
//         // Student
//         //    ↓
//         // Guardian
//         //    ↓
//         // User
//         //
//         // Role tidak di-ThenInclude karena sebelumnya menyebabkan
//         // nullable warning.
//         // =========================================================

//         var student = await _context.Students
//             .Include(x => x.Guardian)
//                 .ThenInclude(x => x.User)
//             .FirstOrDefaultAsync(x =>
//                 x.NIS == request.Username &&
//                 x.IsActive &&
//                 !x.IsDeleted);

//         // =========================================================
//         // NIS TIDAK DITEMUKAN
//         // =========================================================

//         if (student == null)
//         {
//             throw new UnauthorizedException(
//                 "Username atau password salah.");
//         }

//         // =========================================================
//         // AMBIL GUARDIAN
//         // =========================================================

//         var guardian = student.Guardian;

//         if (guardian == null)
//         {
//             throw new UnauthorizedException(
//                 "Data wali murid tidak ditemukan.");
//         }

//         // =========================================================
//         // CHECK GUARDIAN ACTIVE
//         // =========================================================

//         if (!guardian.IsActive)
//         {
//             throw new UnauthorizedException(
//                 "Wali murid tidak aktif.");
//         }

//         if (guardian.IsDeleted)
//         {
//             throw new UnauthorizedException(
//                 "Wali murid tidak aktif.");
//         }

//         // =========================================================
//         // AMBIL USER DARI GUARDIAN
//         // =========================================================

//         var user = guardian.User;

//         if (user == null)
//         {
//             throw new UnauthorizedException(
//                 "Username atau password salah.");
//         }

//         // =========================================================
//         // CHECK USER ACTIVE
//         // =========================================================

//         if (!user.IsActive)
//         {
//             throw new UnauthorizedException(
//                 "User tidak aktif.");
//         }

//         if (user.IsDeleted)
//         {
//             throw new UnauthorizedException(
//                 "User tidak aktif.");
//         }

//         // =========================================================
//         // AMBIL ROLE
//         //
//         // Dilakukan SEBELUM password verification supaya kita bisa
//         // membedakan user yang bukan Guardian saat testing.
//         // =========================================================

//         var roleEntity = await _context.Roles
//             .FirstOrDefaultAsync(x =>
//                 x.Id == user.RoleId &&
//                 !x.IsDeleted);

//         // =========================================================
//         // ROLE TIDAK DITEMUKAN
//         // =========================================================

//         if (roleEntity == null)
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak valid.");
//         }

//         if (string.IsNullOrWhiteSpace(roleEntity.Name))
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak valid.");
//         }

//         // =========================================================
//         // CHECK ROLE ACTIVE
//         // =========================================================

//         if (!roleEntity.IsActive)
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak aktif.");
//         }

//         if (roleEntity.IsDeleted)
//         {
//             throw new UnauthorizedException(
//                 "Role user tidak aktif.");
//         }

//         var role = roleEntity.Name
//             .Trim()
//             .ToUpperInvariant();

//         // =========================================================
//         // MOBILE HANYA GUARDIAN
//         //
//         // PENTING:
//         // Validasi role dilakukan SEBELUM password.
//         // =========================================================

//         if (role != "GUARDIAN")
//         {
//             throw new UnauthorizedException(
//                 "Hanya Wali Murid yang dapat login melalui aplikasi mobile.");
//         }

//         // =========================================================
//         // VERIFY PASSWORD
//         // =========================================================

//         if (!_passwordHasher.Verify(
//                 request.Password,
//                 user.PasswordHash))
//         {
//             throw new UnauthorizedException(
//                 "Username atau password salah.");
//         }

//         // =========================================================
//         // UPDATE LAST LOGIN
//         // =========================================================

//         user.LastLogin = DateTime.UtcNow;

//         await _context.SaveChangesAsync();

//         // =========================================================
//         // GENERATE JWT
//         //
//         // sub         = User.Id
//         // unique_name = NIS
//         // guardian_id = Guardian.Id
//         // nis         = Student.NIS
//         // =========================================================

//         var jwt = _jwtTokenGenerator.GenerateToken(
//             user,
//             ClientType.MOBILE,
//             guardian.Id,
//             student.NIS);

//         // =========================================================
//         // RESPONSE
//         // =========================================================

//         return new LoginResponse
//         {
//             UserId = user.Id,

//             // Login identifier Mobile adalah NIS
//             Username = student.NIS,

//             FullName = user.FullName,
//             Role = roleEntity.Name,
//             ClientType = ClientType.MOBILE.ToString(),
//             Token = jwt.Token,
//             ExpiresAt = jwt.ExpiresAt
//         };
//     }
// }

using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Features.Authentication.Login;
using SmartSchool.Domain.Entities;
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
        // ============================================================
        // 1. VALIDASI CLIENT TYPE
        // ============================================================

        if (!Enum.IsDefined(typeof(ClientType), request.ClientType))
        {
            throw new UnauthorizedException(
                "Client type tidak valid.");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new UnauthorizedException(
                "Username atau NIS wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedException(
                "Password wajib diisi.");
        }

        // ============================================================
        // 2. VARIABLE UNTUK MENYIMPAN HASIL RESOLVE
        // ============================================================

        User? user = null;
        Student? student = null;
        Guardian? guardian = null;

        // ============================================================
        // 3. COBA CARI BERDASARKAN USERNAME
        //
        // Contoh:
        // admin
        // guardian_GDN001
        // ============================================================

        user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Username == request.Username &&
                !x.IsDeleted);

        // ============================================================
        // 4. JIKA USERNAME TIDAK DITEMUKAN,
        //    COBA CARI BERDASARKAN NIS
        //
        // NIS
        //   ↓
        // Student
        //   ↓
        // Guardian
        //   ↓
        // User
        // ============================================================

        if (user == null)
        {
            student = await _context.Students
                .Include(x => x.Guardian)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.NIS == request.Username &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (student != null)
            {
                guardian = student.Guardian;

                if (guardian != null)
                {
                    user = guardian.User;
                }
            }
        }

        // ============================================================
        // 5. USER TIDAK DITEMUKAN
        // ============================================================

        if (user == null)
        {
            throw new UnauthorizedException(
                "Username atau password salah.");
        }

        // ============================================================
        // 6. VALIDASI USER
        // ============================================================

        if (user.IsDeleted)
        {
            throw new UnauthorizedException(
                "User tidak aktif.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException(
                "User tidak aktif.");
        }

        // ============================================================
        // 7. AMBIL ROLE SECARA LANGSUNG BERDASARKAN RoleId
        //
        // Jangan menggunakan:
        //
        // var roleEntity = user.Role;
        //
        // karena navigation User -> Role bisa tidak ter-load.
        // ============================================================

        var roleEntity = await _context.Roles
            .FirstOrDefaultAsync(x =>
                x.Id == user.RoleId &&
                !x.IsDeleted);

        if (roleEntity == null)
        {
            throw new UnauthorizedException(
                "Role user tidak valid.");
        }

        // ============================================================
        // 8. VALIDASI NAMA ROLE
        // ============================================================

        if (string.IsNullOrWhiteSpace(roleEntity.Name))
        {
            throw new UnauthorizedException(
                "Role user tidak valid.");
        }

        // ============================================================
        // 9. VALIDASI STATUS ROLE
        // ============================================================

        if (!roleEntity.IsActive)
        {
            throw new UnauthorizedException(
                "Role user tidak aktif.");
        }

        if (roleEntity.IsDeleted)
        {
            throw new UnauthorizedException(
                "Role user tidak aktif.");
        }

        var role = roleEntity.Name
            .Trim()
            .ToUpperInvariant();

        // ============================================================
        // 10. VALIDASI AKSES BERDASARKAN CLIENT TYPE
        // ============================================================

        // ------------------------------------------------------------
        // GUARDIAN -> WEB
        // ------------------------------------------------------------

        if (role == "GUARDIAN" &&
            request.ClientType == ClientType.WEB)
        {
            throw new UnauthorizedException(
                "Wali Murid hanya dapat login melalui aplikasi mobile.");
        }

        // ------------------------------------------------------------
        // ADMIN -> MOBILE
        // ------------------------------------------------------------

        if (role == "ADMIN" &&
            request.ClientType == ClientType.MOBILE)
        {
            throw new UnauthorizedException(
                "Admin hanya dapat login melalui aplikasi web.");
        }

        // ------------------------------------------------------------
        // WEB HANYA UNTUK ADMIN
        // ------------------------------------------------------------

        if (request.ClientType == ClientType.WEB &&
            role != "ADMIN")
        {
            throw new UnauthorizedException(
                "User tidak memiliki akses ke aplikasi web.");
        }

        // ------------------------------------------------------------
        // MOBILE HANYA UNTUK GUARDIAN
        // ------------------------------------------------------------

        if (request.ClientType == ClientType.MOBILE &&
            role != "GUARDIAN")
        {
            throw new UnauthorizedException(
                "User tidak memiliki akses ke aplikasi mobile.");
        }

        // ============================================================
        // 11. VALIDASI PASSWORD
        // ============================================================

        if (!_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedException(
                "Username atau password salah.");
        }

        // ============================================================
        // 12. UPDATE LAST LOGIN
        // ============================================================

        user.LastLogin = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ============================================================
        // 13. LOGIN MOBILE / GUARDIAN
        // ============================================================

        if (request.ClientType == ClientType.MOBILE)
        {
            // Pastikan student ditemukan
            if (student == null)
            {
                throw new UnauthorizedException(
                    "Data siswa tidak ditemukan.");
            }

            // Pastikan guardian ditemukan
            if (guardian == null)
            {
                throw new UnauthorizedException(
                    "Data wali murid tidak ditemukan.");
            }

            // --------------------------------------------------------
            // VALIDASI GUARDIAN
            // --------------------------------------------------------

            if (guardian.IsDeleted)
            {
                throw new UnauthorizedException(
                    "Wali murid tidak aktif.");
            }

            if (!guardian.IsActive)
            {
                throw new UnauthorizedException(
                    "Wali murid tidak aktif.");
            }

            // --------------------------------------------------------
            // VALIDASI USER ID GUARDIAN
            // --------------------------------------------------------

            if (!guardian.UserId.HasValue)
            {
                throw new UnauthorizedException(
                    "Akun wali murid belum terhubung.");
            }

            if (guardian.UserId.Value != user.Id)
            {
                throw new UnauthorizedException(
                    "Data akun wali murid tidak valid.");
            }

            // ========================================================
            // GENERATE JWT MOBILE
            // ========================================================

            var jwt = _jwtTokenGenerator.GenerateToken(
                user,
                ClientType.MOBILE,
                guardian.Id,
                student.NIS);

            return new LoginResponse
            {
                UserId = user.Id,
                Username = student.NIS,
                FullName = user.FullName,
                Role = roleEntity.Name,
                ClientType = ClientType.MOBILE.ToString(),
                Token = jwt.Token,
                ExpiresAt = jwt.ExpiresAt
            };
        }

        // ============================================================
        // 14. LOGIN WEB / ADMIN
        // ============================================================

        var webJwt = _jwtTokenGenerator.GenerateToken(
            user,
            ClientType.WEB);

        return new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = roleEntity.Name,
            ClientType = ClientType.WEB.ToString(),
            Token = webJwt.Token,
            ExpiresAt = webJwt.ExpiresAt
        };
    }
}