using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Http;
using CarRental.BLL.DTOs.Car;

namespace CarRental.Web.ViewModels.Admin;

public class CarEditViewModel
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Марка обязательна")]
    [Display(Name = "Марка")]
    public string Brand { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Модель обязательна")]
    [Display(Name = "Модель")]
    public string Model { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Год обязателен")]
    [Range(2000, 2100, ErrorMessage = "Год должен быть от 2000 до 2100")]
    [Display(Name = "Год выпуска")]
    public int Year { get; set; }
    
    [Display(Name = "Цвет")]
    public string Color { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Цена за сутки обязательна")]
    [Range(1, 1000000, ErrorMessage = "Цена должна быть положительной")]
    [Display(Name = "Цена за сутки (₽)")]
    public decimal PricePerDay { get; set; }
    
    [Display(Name = "Доступен для аренды")]
    public bool IsAvailable { get; set; }
    
    [Display(Name = "Фотографии")]
    public List<IFormFile>? ImageFiles { get; set; }

    public List<CarImageDto> ExistingImages { get; set; } = new();
    
    
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Класс обязателен")]
    [Display(Name = "Класс")]
    public CarClass? Class { get; set; }
    
    [Required(ErrorMessage = "Коробка передач обязательна")]
    [Display(Name = "Коробка передач")]
    public TransmissionType? Transmission { get; set; }
    
    [Required(ErrorMessage = "Тип топлива обязателен")]
    [Display(Name = "Тип топлива")]
    public FuelType? FuelType { get; set; }
    
    [Required(ErrorMessage = "Количество мест обязательно")]
    [Range(1, 20, ErrorMessage = "Количество мест от 1 до 20")]
    [Display(Name = "Количество мест")]
    public int Seats { get; set; }
    
    [Required(ErrorMessage = "Объем двигателя обязателен")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Введите корректное число (например: 2.0 или 2.5)")]
    [Display(Name = "Объем двигателя (л)")]
    public string EngineCapacityString { get; set; } = "2.0";
    
    // Дополнительное свойство для преобразования
    public double EngineCapacity 
    {
        get 
        {
            if (double.TryParse(EngineCapacityString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return 2.0;
        }
    }
    
    // Для dropdown списков
    public List<CarClass> CarClasses { get; set; } = Enum.GetValues<CarClass>().ToList();
    public List<FuelType> FuelTypes { get; set; } = Enum.GetValues<FuelType>().ToList();
    public List<TransmissionType> TransmissionTypes { get; set; } = Enum.GetValues<TransmissionType>().ToList();
    
    // Список цветов
    public List<string> AvailableColors { get; set; } = new List<string>
    {
        "Белый", "Черный", "Серый", "Серебристый",
        "Красный", "Синий", "Зеленый", "Желтый",
        "Оранжевый", "Коричневый", "Бежевый", "Фиолетовый"
    };
    
    public string? CurrentImageUrl { get; set; }
    [Display(Name = "Госномер")]
    [StringLength(20, ErrorMessage = "Госномер не может быть длиннее 20 символов")]
    public string? LicensePlate { get; set; }
    
    [Display(Name = "VIN")]
    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен содержать 17 символов")]
    public string? VIN { get; set; }

    [Display(Name = "Цена от 15 суток (₽)")]
    [Range(0, 1000000, ErrorMessage = "Цена должна быть от 0 до 1 000 000")]
    public decimal PricePerDay15 { get; set; }

    [Display(Name = "Цена от 30 суток (₽)")]
    [Range(0, 1000000, ErrorMessage = "Цена должна быть от 0 до 1 000 000")]
    public decimal PricePerDay30 { get; set; }

    [Display(Name = "Депозит (₽)")]
    [Range(0, 1000000, ErrorMessage = "Депозит должен быть от 0 до 1 000 000")]
    public decimal Deposit { get; set; }

    [Display(Name = "Лимит пробега в сутки (км)")]
    [Range(0, 10000, ErrorMessage = "Лимит пробега от 0 до 10000 км")]
    public int MileageLimitPerDay { get; set; } = 250;

    [Display(Name = "Доплата за перепробег (₽)")]
    [Range(0, 100000, ErrorMessage = "Цена от 0 до 100000 ₽")] 
    public decimal OverMileagePricePerKm { get; set; }

    [Display(Name = "Стоимость безлимитного пробега (₽)")]
    [Range(0, 100000, ErrorMessage = "Цена от 0 до 100000 ₽")]
    public decimal UnlimitedMileagePrice { get; set; }
}