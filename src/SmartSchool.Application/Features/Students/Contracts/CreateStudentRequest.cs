using SmartSchool.Domain.Enums;

namespace SmartSchool.Application.Features.Students.Contracts;

public class CreateStudentRequest
{
    public string NIS { get; set; } = string.Empty;

    public string? NISN { get; set; }

    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public string? BirthPlace { get; set; }

    public DateTime BirthDate { get; set; }

    public string? Address { get; set; }

    public string? PhotoUrl { get; set; }

    // public Guid ClassRoomId { get; set; }

    // public Guid GuardianId { get; set; }
     public string ClassRoomCode { get; set; } = string.Empty;

  public string AcademicYear { get; set; } = string.Empty;
  
    public string GuardianCode { get; set; } = string.Empty;

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}