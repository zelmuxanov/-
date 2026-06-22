using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CarRental.BLL.DTOs.Car;

public class CarUpdateDto
{
    [StringLength(50, ErrorMessage = "Марка не может быть длиннее 50 символов")]
    public string? Brand { get; set; }
    
    [StringLength(50, ErrorMessage = "Модель не может быть длиннее 50 символов")]
    public string? Model { get; set; }
    
    [Range(1900, 2100, ErrorMessage = "Год должен быть между 1900 и 2100")]
    public int? Year { get; set; }
    
    public string? Color { get; set; }
    
    [Range(1, 1000000, ErrorMessage = "Цена должна быть от 1 до 1 000 000")]
    public decimal? PricePerDay { get; set; }
    
    public bool? IsAvailable { get; set; }
    
    [StringLength(1000, ErrorMessage = "Описание не может быть длиннее 1000 символов")]
    public string? Description { get; set; }
    
    public CarClass? Class { get; set; }
    
    public TransmissionType? Transmission { get; set; }
    
    public FuelType? FuelType { get; set; }
    
    [Range(1, 20, ErrorMessage = "Количество мест должно быть от 1 до 20")]
    public int? Seats { get; set; }
    
    [Range(0.5, 10.0, ErrorMessage = "Объем двигателя должен быть от 0.5 до 10.0 л")]
    public double? EngineCapacity { get; set; }
    
    public List<IFormFile>? ImageFiles { get; set; }
    public string? LicensePlate { get; set; }
    public string? VIN { get; set; }
    [Range(0, 1000000)]
    public decimal? PricePerDay15 { get; set; }

    [Range(0, 1000000)]
    public decimal? PricePerDay30 { get; set; }

    [Range(0, 1000000)]
    public decimal? Deposit { get; set; }

    [Range(0, 10000)]
    public int? MileageLimitPerDay { get; set; }

    [Range(0, 1000)]
    public decimal? OverMileagePricePerKm { get; set; }

    [Range(0, 1000)]
    public decimal? UnlimitedMileagePrice { get; set; }
}