using System.ComponentModel.DataAnnotations;
using SmartSchool.Domain.Enums;
public class CreateGuardianRequest
{
    [Required(ErrorMessage = "Guardian code wajib diisi.")]
    [MaxLength(30, ErrorMessage = "Guardian code maksimal 30 karakter.")]
    public string GuardianCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public GuardianRelationship Relationship { get; set; }

    public string? Occupation { get; set; }
}