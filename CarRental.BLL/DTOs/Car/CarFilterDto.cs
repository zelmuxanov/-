namespace CarRental.BLL.DTOs.Car;

public class CarFilterDto
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinSeats { get; set; }
    public int? MaxSeats { get; set; }
    public string? SortBy { get; set; }
    public string? Class { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
}