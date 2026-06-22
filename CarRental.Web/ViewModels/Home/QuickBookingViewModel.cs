using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Home;

public class QuickBookingViewModel
{
    [Required(ErrorMessage = "Укажите ваше имя")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Укажите телефон")]
    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Укажите email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Выберите дату начала")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Выберите дату окончания")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Выберите автомобиль")]
    public Guid CarId { get; set; }

    public bool UnlimitedMileage { get; set; }
    public string? DeliveryLocation { get; set; }
}