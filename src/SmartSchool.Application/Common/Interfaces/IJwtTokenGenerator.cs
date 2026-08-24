using SmartSchool.Domain.Entities;
using SmartSchool.Application.Features.Authentication.Login;

namespace SmartSchool.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, ClientType clientType);
}