using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CarRental.Web.ViewModels.Admin;

public class GeneralSettingsViewModel
{
    [Required(ErrorMessage = "Название компании обязательно")]
    [Display(Name = "Название компании")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон обязателен")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Адрес обязателен")]
    [Display(Name = "Адрес")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Логотип")]
    public IFormFile? LogoFile { get; set; }

    public string? CurrentLogoUrl { get; set; }

    [Display(Name = "Ссылка на WhatsApp")]
    [Url(ErrorMessage = "Введите корректную ссылку")]
    public string WhatsAppUrl { get; set; } = string.Empty;

    [Display(Name = "Ссылка на Telegram")]
    [Url(ErrorMessage = "Введите корректную ссылку")]
    public string TelegramUrl { get; set; } = string.Empty;

    [Display(Name = "Ссылка на МАХ")]
    [Url(ErrorMessage = "Введите корректную ссылку")]
    public string MaxUrl { get; set; } = string.Empty;  // ← добавлено
}