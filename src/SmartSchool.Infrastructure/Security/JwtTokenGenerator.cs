// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.Extensions.Options;
// using Microsoft.IdentityModel.Tokens;
// using SmartSchool.Application.Common.Interfaces;
// using SmartSchool.Application.Common.Settings;
// using SmartSchool.Domain.Entities;

// namespace SmartSchool.Infrastructure.Security;

// public class JwtTokenGenerator : IJwtTokenGenerator
// {
//     private readonly JwtSettings _jwt;

//     public JwtTokenGenerator(IOptions<JwtSettings> options)
//     {
//         _jwt = options.Value;
//     }

//     public string GenerateToken(User user)
//     {
//         var key = new SymmetricSecurityKey(
//             Encoding.UTF8.GetBytes(_jwt.Key));

//         var credentials = new SigningCredentials(
//             key,
//             SecurityAlgorithms.HmacSha256);

//         var claims = new List<Claim>
//         {
//             new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//             new(JwtRegisteredClaimNames.UniqueName, user.Username),
//             new(ClaimTypes.Name, user.FullName),
//             new(ClaimTypes.Role, user.Role.Name)
//         };

//         var token = new JwtSecurityToken(
//             issuer: _jwt.Issuer,
//             audience: _jwt.Audience,
//             claims: claims,
//             expires: DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes),
//             signingCredentials: credentials);

//         return new JwtSecurityTokenHandler()
//             .WriteToken(token);
//     }
// }
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Common.Settings;
using SmartSchool.Application.Features.Authentication.Login;
using SmartSchool.Domain.Entities;

namespace SmartSchool.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwt;

    public JwtTokenGenerator(IOptions<JwtSettings> options)
    {
        _jwt = options.Value;
    }

    public string GenerateToken(User user, ClientType clientType)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.Name),

            // Client yang digunakan saat login
            new("client_type", clientType.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}