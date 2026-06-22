using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Пароль' обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}