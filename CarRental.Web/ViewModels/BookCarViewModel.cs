using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Car;

public class BookCarViewModel : IValidatableObject
{
    [Required]
    public Guid CarId { get; set; }
    
    [Required(ErrorMessage = "Дата начала аренды обязательна")]
    [Display(Name = "Дата начала")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    [Required(ErrorMessage = "Дата окончания аренды обязательна")]
    [Display(Name = "Дата окончания")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    [Display(Name = "Нужна доставка?")]
    public bool NeedDelivery { get; set; }

    [Display(Name = "Куда доставить?")]
    public DeliveryLocation? DeliveryLocation { get; set; }

    [Display(Name = "Безлимитный пробег (+2000 ₽/сутки)")]
    public bool UnlimitedMileage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate < DateTime.Today)
        {
            yield return new ValidationResult("Дата начала не может быть раньше завтрашнего дня", new[] { nameof(StartDate) });
        }

        if (StartDate >= EndDate)
        {
            yield return new ValidationResult("Дата окончания должна быть позже даты начала", new[] { nameof(EndDate) });
        }

        var rentalDays = (EndDate - StartDate).Days;
        if (rentalDays < 1)
        {
            yield return new ValidationResult("Минимальный срок аренды - от 1 дня", new[] { nameof(EndDate) });
        }

        if (NeedDelivery && !DeliveryLocation.HasValue)
        {
            yield return new ValidationResult("Укажите место доставки", new[] { nameof(DeliveryLocation) });
        }
    }
}

public enum DeliveryLocation
{
    [Display(Name = "Аэропорт Шереметьево - 4500 ₽")]
    Sheremetyevo,
    [Display(Name = "Аэропорт Внуково - 3500 ₽")]
    Vnukovo,
    [Display(Name = "Аэропорт Домодедово - 6000 ₽")]
    Domodedovo,
    [Display(Name = "По Москве и МО (индивидуально)")]
    Moscow
}