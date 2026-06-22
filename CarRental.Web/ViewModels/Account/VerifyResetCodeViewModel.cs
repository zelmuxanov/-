using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Account;

public class VerifyResetCodeViewModel
{
    [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Код подтверждения' обязательно для заполнения")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен содержать 6 цифр")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Код должен содержать только цифры")]
    [Display(Name = "Код подтверждения")]
    public string Code { get; set; } = string.Empty;
}