using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CarRental.BLL.DTOs.Car;

public class CarCreateDto
{
    [Required]
    public string Brand { get; set; } = string.Empty;
    
    [Required]
    public string Model { get; set; } = string.Empty;
    
    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }
    
    public string Color { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 1000000)]
    public decimal PricePerDay { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public CarClass Class { get; set; }
    
    [Required]
    public TransmissionType Transmission { get; set; }
    
    [Required]
    public FuelType FuelType { get; set; }
    
    [Required]
    [Range(1, 20)]
    public int Seats { get; set; }
    
    [Required]
    [Range(0.5, 10.0)]
    public double EngineCapacity { get; set; }
    
    public List<IFormFile>? ImageFiles { get; set; }
    public string? LicensePlate { get; set; }
    public string? VIN { get; set; }

    [Range(0, 1000000)]
    public decimal PricePerDay15 { get; set; }

    [Range(0, 1000000)]
    public decimal PricePerDay30 { get; set; }

    [Range(0, 1000000)]
    public decimal Deposit { get; set; }

    [Range(0, 10000)]
    public int MileageLimitPerDay { get; set; } = 250;

    [Range(0, 1000)]
    public decimal OverMileagePricePerKm { get; set; }

    [Range(0, 1000)]
    public decimal UnlimitedMileagePrice { get; set; }
}