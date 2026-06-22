using System.ComponentModel.DataAnnotations;

namespace CarRental.BLL.Validators;

public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime birthDate)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate > DateTime.Today.AddYears(-age)) age--;

            if (age < _minimumAge)
            {
                return new ValidationResult(ErrorMessage ?? $"Минимальный возраст - {_minimumAge} лет");
            }
        }

        return ValidationResult.Success;
    }
}