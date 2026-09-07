using SmartSchool.Domain.Entities;
using SmartSchool.Application.Features.Authentication.Login;

namespace SmartSchool.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(
        User user,
        ClientType clientType,
        Guid? guardianId = null,
        string? nis = null);
}

public class JwtTokenResult
{
    public string Token { get; init; } = string.Empty;

    public DateTime ExpiresAt { get; init; }
}