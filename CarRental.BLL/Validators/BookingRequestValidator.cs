using FluentValidation;
using CarRental.BLL.DTOs.Booking;

namespace CarRental.BLL.Validators;

public class BookingRequestValidator : AbstractValidator<BookingRequestDto>
{
    public BookingRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CarId)
            .NotEmpty().WithMessage("Car ID is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThan(DateTime.Now).WithMessage("Start date must be in the future");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
    }
}
