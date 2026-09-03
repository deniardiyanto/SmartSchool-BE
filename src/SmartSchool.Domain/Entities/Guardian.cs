using SmartSchool.Domain.Common;
using SmartSchool.Domain.Enums;

namespace SmartSchool.Domain.Entities;

public class Guardian : BaseAuditableEntity
{
   public Guid? UserId { get; set; }

    public string GuardianCode { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public GuardianRelationship Relationship { get; set; }

    public string? Occupation { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public User? User { get; set; }

    public ICollection<Student> Students { get; set; }
        = new List<Student>();
}