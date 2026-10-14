using Assessment.Application.DTOs.Donors;
using FluentValidation;

namespace Assessment.Application.Validators;

public class CreateDonorDtoValidator : AbstractValidator<CreateDonorDto>
{
    public CreateDonorDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[0-9\+\-\s\(\)]+$").WithMessage("Phone number format is invalid.");
    }
}
