using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Controllers.Api;

[ApiController]
[Route("api/public")]
public class PublicApiController : ControllerBase
{
    private readonly ICarService _carService;

    public PublicApiController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet("free-cars")]
    public async Task<IActionResult> GetFreeCars(DateTime startDate, DateTime endDate)
    {
        // Приводим даты к UTC (начало дня UTC)
        var utcStart = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var utcEnd = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

        var allCars = await _carService.GetAvailableCarsAsync();
        var freeCars = new List<object>();

        foreach (var car in allCars)
        {
            var isAvailable = await _carService.IsCarAvailableAsync(car.Id, utcStart, utcEnd);
            if (isAvailable)
            {
                freeCars.Add(new
                {
                    car.Id,
                    car.Brand,
                    car.Model,
                    car.Year,
                    car.PricePerDay,
                    car.MainImageUrl,
                    car.LicensePlate
                });
            }
        }

        return Ok(freeCars);
    }
}