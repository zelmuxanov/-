using System.ComponentModel.DataAnnotations;
using CarRental.BLL.DTOs.User;
using CarRental.BLL.DTOs.Booking;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.Validators;

namespace CarRental.Web.ViewModels.Profile;

public class ProfileViewModel
{
    public UserDto UserProfile { get; set; } = new();
    public UserVerificationInfoDto VerificationInfo { get; set; } = new();
    public IEnumerable<BookingDto> UpcomingBookings { get; set; } = new List<BookingDto>();
    public int TotalBookingsCount { get; set; }
    public int CalculatedDrivingExperience { get; set; }
    public int CompletedBookingsCount { get; set; }
}

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Поле 'Имя' обязательно для заполнения")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Фамилия' обязательно для заполнения")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;
    [Display(Name = "Отчество")] 
    public string? MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле 'Телефон' обязательно для заполнения")]
    [RussianPhone(ErrorMessage = "Некорректный формат российского номера телефона")]
    [Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Текущий пароль обязателен")]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новый пароль обязателен")]
    [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
public class DocumentsViewModel
{
    public UserDocumentsDto Documents { get; set; } = new();
    public bool CanUploadDocuments { get; set; } = true;
    public string? UploadErrorMessage { get; set; }
}

public class DocumentUploadViewModel
{
    [Required(ErrorMessage = "Номер водительского удостоверения обязателен")]
    [Display(Name = "Номер водительского удостоверения")]
    public string DriverLicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Срок действия обязателен")]
    [Display(Name = "Срок действия")]
    [DataType(DataType.Date)]
    public DateTime? DriverLicenseExpiry { get; set; }

    [Display(Name = "Скан паспорта")]
    public IFormFile? PassportScan { get; set; }

    [Display(Name = "Скан водительского удостоверения")]
    public IFormFile? DriverLicenseScan { get; set; }
}