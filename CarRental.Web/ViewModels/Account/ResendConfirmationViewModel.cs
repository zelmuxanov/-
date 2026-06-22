using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Account;

public class ResendConfirmationViewModel
{
    [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}