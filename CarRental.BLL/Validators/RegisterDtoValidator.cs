using FluentValidation;
using CarRental.BLL.DTOs.User;

namespace CarRental.BLL.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 50);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 50);
        RuleFor(x => x.MiddleName).NotEmpty().Length(2, 50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.BirthDate).NotEmpty().Must(BeAtLeast23).WithMessage("Возраст должен быть не менее 23 лет");
        RuleFor(x => x.DrivingExperience).NotEmpty().GreaterThanOrEqualTo(2).WithMessage("Стаж вождения должен быть не менее 2 лет");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }

    private bool BeAtLeast23(DateTime? birthDate)
    {
        if (!birthDate.HasValue) return false;
        
        var age = DateTime.Today.Year - birthDate.Value.Year;
        if (birthDate.Value > DateTime.Today.AddYears(-age)) age--;
        return age >= 23;
    } 
}