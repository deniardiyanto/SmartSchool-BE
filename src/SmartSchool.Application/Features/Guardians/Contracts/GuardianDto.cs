using SmartSchool.Domain.Enums;
public class GuardianDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public GuardianRelationship Relationship { get; set; }

    public string? Occupation { get; set; }

    public bool IsActive { get; set; }

    public string GuardianCode { get; set; } = null!;
}