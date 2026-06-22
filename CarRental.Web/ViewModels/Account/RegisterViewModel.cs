using System.ComponentModel.DataAnnotations;
using CarRental.BLL.Validators;

namespace CarRental.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Поле 'Имя' обязательно для заполнения")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Фамилия' обязательно для заполнения")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;
    [Display(Name = "Отчество")] 
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Телефон' обязательно для заполнения")]
    [RussianPhone(ErrorMessage = "Некорректный формат российского номера телефона")]
    [Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Дата рождения' обязательно для заполнения")]
    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; } 

    [Required(ErrorMessage = "Поле 'Стаж вождения' обязательно для заполнения")]
    [Range(2, 100, ErrorMessage = "Стаж вождения должен быть не менее 2 лет")]
    [Display(Name = "Стаж вождения (лет)")]
    public int? DrivingExperience { get; set; } 

    [Required(ErrorMessage = "Поле 'Пароль' обязательно для заполнения")]
    [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Подтверждение пароля' обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вы должны принять условия соглашения")]
    [Display(Name = "Я принимаю условия соглашения")]
    public bool AgreeToTerms { get; set; }
}