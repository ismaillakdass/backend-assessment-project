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

        // TODO [Task 7]: Add missing validation rules here.
        // Currently amounts <= 0 or future dates are accepted without error!
    }
}
