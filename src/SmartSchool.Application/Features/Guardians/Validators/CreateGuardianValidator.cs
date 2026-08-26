using FluentValidation;
using SmartSchool.Application.Features.Guardians.Contracts;

namespace SmartSchool.Application.Features.Guardians.Validators;

public class CreateGuardianValidator : AbstractValidator<CreateGuardianRequest>
{
    public CreateGuardianValidator()
    {
        RuleFor(x => x.GuardianCode)
    .NotEmpty()
    .WithMessage("Guardian code wajib diisi.")
    .MaximumLength(30)
    .WithMessage("Guardian code maksimal 30 karakter.");
    
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .MaximumLength(100)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(255);

        RuleFor(x => x.Occupation)
            .MaximumLength(100);

        RuleFor(x => x.Relationship)
            .IsInEnum();
    }
}