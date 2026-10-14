using Assessment.Application.DTOs.Donations;
using FluentValidation;

namespace Assessment.Application.Validators;

/// <summary>
/// TASK 7: FluentValidation
/// This validator currently lacks critical business validation rules.
/// Missing:
/// - Amount must be strictly greater than 0
/// - DonationDate cannot be in the future (must be <= DateTime.UtcNow)
/// - DonationType must be a valid enum value
/// - Notes (if provided) cannot exceed 500 characters
/// </summary>
public class CreateDonationDtoValidator : AbstractValidator<CreateDonationDto>
{
    public CreateDonationDtoValidator()
    {
        RuleFor(x => x.DonorId)
            .GreaterThan(0)
            .WithMessage("Valid DonorId is required.");
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Sıfırdan büyük olmalı.");
        RuleFor(x => x.DonationType)
            .IsInEnum()
            .WithMessage("DonationType must be a valid value.");

        RuleFor(x => x.DonationDate)
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("DonationDate cannot be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters.");

        // TODO [Task 7]: Add missing validation rules here.
        // Currently amounts <= 0 or future dates are accepted without error!
    }
}
