using System.ComponentModel.DataAnnotations;

namespace SmartSchool.Application.Features.Authentication.Login;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(ClientType))]
    public ClientType ClientType { get; set; }
}