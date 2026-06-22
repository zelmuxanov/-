using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CarRental.BLL.Validators;

public class RussianPhoneAttribute : ValidationAttribute
{
    private static readonly Regex _phoneRegex = new Regex(
        @"^(\+7|7|8)?[\s\-]?\(?[489][0-9]{2}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$",
        RegexOptions.Compiled
    );

    public RussianPhoneAttribute() : base("Номер телефона должен быть в российском формате: +7 (XXX) XXX-XX-XX")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return false;
        
        var phone = value.ToString();
        if (string.IsNullOrWhiteSpace(phone)) return false;

        // Убираем все нецифровые символы
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        
        // Проверяем различные форматы российских номеров
        return (digitsOnly.StartsWith("79") && digitsOnly.Length == 11) || // +7 9XX...
            (digitsOnly.StartsWith("89") && digitsOnly.Length == 11) || // 8 9XX...
            (digitsOnly.StartsWith("9") && digitsOnly.Length == 10) ||  // 9XX... (без кода)
            (digitsOnly.StartsWith("7") && digitsOnly.Length == 11) ||  // 7 9XX...
            (digitsOnly.StartsWith("+79") && digitsOnly.Length == 12);  // +7 9XX...
    }

    public override string FormatErrorMessage(string name)
    {
        return $"Поле {name} должно содержать российский номер телефона в формате: +7 (XXX) XXX-XX-XX";
    }
}