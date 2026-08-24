namespace SmartSchool.Application.Features.Authentication.Login;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public ClientType ClientType { get; set; }
}