using AutoMapper;
using CarRental.BLL.DTOs.Booking;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Bookings")]
public class BookingsController : BaseAdminController
{
    private readonly IBookingService _bookingService;
    private readonly IContractService _contractService;
    private readonly IEmailService _emailService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IBookingService bookingService,
        IContractService contractService,
        IEmailService emailService,
        IBookingRepository bookingRepository,
        IMapper mapper,
        ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _contractService = contractService;
        _emailService = emailService;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(BookingStatus? status = null)
    {
        try
        {
            ViewData["Title"] = "Управление бронированиями";
            
            IEnumerable<CarRental.BLL.DTOs.Booking.BookingDto> bookings;
            
            if (status.HasValue)
            {
                bookings = await _bookingService.GetBookingsByStatusAsync(status.Value);
            }
            else
            {
                bookings = await _bookingService.GetRecentBookingsAsync(50);
            }
            
            ViewBag.CurrentStatus = status;
            return View(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке бронирований");
            TempData["ErrorMessage"] = "Ошибка при загрузке бронирований";
            return View(new List<CarRental.BLL.DTOs.Booking.BookingDto>());
        }
    }

    [HttpPost("Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id, string contractNumber)
    {
        // Загружаем бронирование вместе с машиной и пользователем
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);
        if (booking == null) return NotFound();

        booking.ContractNumber = contractNumber;
        booking.Status = BookingStatus.Confirmed;

        // Генерируем договор (он возвращает относительный путь, например "/uploads/contracts/123.docx")
        var contractUrl = await _contractService.GenerateContractDocxAsync(booking);
        booking.ContractUrl = contractUrl;

        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();

        // Отправляем одно письмо: booking.Car и booking.User уже подгружены
        if (booking.User != null)
            await _emailService.SendBookingConfirmationAsync(booking, booking.User);

        TempData["SuccessMessage"] = "Бронирование подтверждено, договор отправлен клиенту.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Cancel/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            Console.WriteLine($"=== ОТМЕНА БРОНИРОВАНИЯ {id} ===");
            
            var result = await _bookingService.CancelBookingAsync(id);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Бронирование успешно отменено";
                Console.WriteLine($"✅ Бронирование {id} отменено");
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось отменить бронирование";
                Console.WriteLine($"❌ Не удалось отменить бронирование {id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отмене бронирования {BookingId}", id);
            TempData["ErrorMessage"] = $"Ошибка при отмене: {ex.Message}";
            Console.WriteLine($"💥 Ошибка: {ex.Message}");
        }
        
        return RedirectToAction(nameof(Index));
    }
}