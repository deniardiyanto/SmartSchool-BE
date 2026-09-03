using SmartSchool.Domain.Common;

namespace SmartSchool.Domain.Entities;

public class User : BaseAuditableEntity
{
    public Guid RoleId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; } = true;

    public Role Role { get; set; } = null!;

    public Guardian? Guardian { get; set; }
}