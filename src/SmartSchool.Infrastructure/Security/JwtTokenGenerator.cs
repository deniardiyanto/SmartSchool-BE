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

    public JwtTokenResult GenerateToken(
        User user,
        ClientType clientType,
        Guid? guardianId = null,
        string? nis = null)
    {
        var expiresAt = DateTime.UtcNow
            .AddMinutes(_jwt.ExpireMinutes);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // User ID
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            // Login identifier
            new(
                JwtRegisteredClaimNames.UniqueName,
                nis ?? user.Username),

            // Full name
            new(
                ClaimTypes.Name,
                user.FullName),

            // Role
            new(
                ClaimTypes.Role,
                user.Role.Name),

            // WEB / MOBILE
            new(
                "client_type",
                clientType.ToString())
        };

        // =========================================================
        // Guardian-specific claims
        // =========================================================

        if (guardianId.HasValue)
        {
            claims.Add(
                new Claim(
                    "guardian_id",
                    guardianId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(nis))
        {
            claims.Add(
                new Claim(
                    "nis",
                    nis));
        }

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtTokenResult
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token),

            ExpiresAt = expiresAt
        };
    }
}