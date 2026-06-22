using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Booking;
using CarRental.Domain.Enums;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Calendar")]
[Authorize(Roles = "Admin")]
public class CalendarController : BaseAdminController
{
    private readonly IBookingService _bookingService;
    private readonly ICarService _carService;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(IBookingService bookingService, ICarService carService, ILogger<CalendarController> logger)
    {
        _bookingService = bookingService;
        _carService = carService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Календарь бронирования";
        return View();
    }

    [HttpGet("GetEvents")]
    public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
    {
        try
        {
            var events = await _bookingService.GetBookingsForCalendarAsync(start, end);
            // Формат для FullCalendar
            var result = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Start.ToString("yyyy-MM-dd"),
                end = e.End.ToString("yyyy-MM-dd"),
                color = e.Color,
                extendedProps = new
                {
                    status = e.Status,
                    userId = e.UserId,
                    carId = e.CarId,
                    carInfo = e.CarInfo,
                    notes = e.Notes
                }
            });
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки событий календаря");
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet("SearchCars")]
    public async Task<IActionResult> SearchCars(string q)
    {
        var cars = await _carService.SearchCarsAsync(q);
        var result = cars.Select(c => new
        {
            id = c.Id,
            brand = c.Brand,
            model = c.Model,
            year = c.Year,
            color = c.Color,
            licensePlate = c.LicensePlate,
            imageUrl = c.MainImageUrl,
            pricePerDay = c.PricePerDay,
            pricePerDay15 = c.PricePerDay15,
            pricePerDay30 = c.PricePerDay30,
            deposit = c.Deposit,
            mileageLimit = c.MileageLimitPerDay,
            overMileagePrice = c.OverMileagePricePerKm,
            unlimitedMileage = c.UnlimitedMileagePrice

        });
        return Json(result);
    }

    [HttpPost("CreateBooking")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBooking([FromBody] AdminBookingDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Некорректные данные", details = errors });
            }

            // Расчет стоимости можно выполнить здесь или получить из model.TotalPrice (уже рассчитана на клиенте)
            var booking = await _bookingService.CreateAdminBookingAsync(model);
            return Json(new { success = true, bookingId = booking.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания бронирования из календаря");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("DeleteBooking/{id}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        try
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            return Json(new { success = result, message = result ? "Бронь удалена" : "Не удалось удалить" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отмены бронирования {BookingId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }
}