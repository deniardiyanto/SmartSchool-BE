using FluentValidation;
using SmartSchool.Application.Features.Students.Contracts;

namespace SmartSchool.Application.Features.Students.Validators;

public class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.NIS)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.NISN)
            .MaximumLength(20);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BirthPlace)
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .MaximumLength(255);

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(255);

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Birth date cannot be in the future.");

        RuleFor(x => x.ClassRoomCode)
            .NotEmpty();

        RuleFor(x => x.GuardianCode)
            .NotEmpty();

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.EnrollmentDate)
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Enrollment date is invalid.");
    }
}