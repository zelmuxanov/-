using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Пароль' обязательно для заполнения")]
    [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}