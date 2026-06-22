using CarRental.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Domain.Entities;

public class Car : BaseEntity
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    [MaxLength(17)]
    public string? VIN { get; set; }
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public CarClass Class { get; set; }
    public TransmissionType Transmission { get; set; }
    public FuelType FuelType { get; set; }
    public int Seats { get; set; }
    public double EngineCapacity { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<CarImage> Images { get; set; } = new List<CarImage>();
    public string? MainImageUrl => Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl 
                              ?? Images?.FirstOrDefault()?.ImageUrl;
    // Цены для длительной аренды
    public decimal PricePerDay15 { get; set; } // Цена при аренде от 15 суток
    public decimal PricePerDay30 { get; set; } // Цена при аренде от 30 суток

    // Финансовые условия
    public decimal Deposit { get; set; } // Обеспечительный платёж

    // Пробег
    public int MileageLimitPerDay { get; set; } = 250; // Включённый пробег в сутки, км
    public decimal OverMileagePricePerKm { get; set; } // Цена за 1 км перепробега
    public decimal UnlimitedMileagePrice { get; set; }
}
