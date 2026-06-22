using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Enums;

namespace CarRental.Web.Areas.Admin.Controllers;

[Route("Admin")]
public class AdminController : BaseAdminController  
{
    private readonly IUserService _userService;
    private readonly ICarService _carService;
    private readonly IBookingService _bookingService;
    private readonly IDocumentService _documentService;

    public AdminController(
        IUserService userService,
        ICarService carService,
        IBookingService bookingService,
        IDocumentService documentService)
    {
        _userService = userService;
        _carService = carService;
        _bookingService = bookingService;
        _documentService = documentService;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            ViewData["Title"] = "Панель управления";
            
            // Получаем реальную статистику
            var allUsers = await _userService.GetAllUsersAsync();
            var allCars = await _carService.GetAllCarsAsync();
            var pendingBookings = await _bookingService.GetBookingsByStatusAsync(BookingStatus.Pending);
            var recentBookings = await _bookingService.GetRecentBookingsAsync(5);
            var pendingDocuments = await _documentService.GetPendingDocumentsAsync();
            var recentUsers = allUsers.OrderByDescending(u => u.RegistrationDate).Take(5);
            
            ViewBag.TotalUsers = allUsers.Count();
            ViewBag.ActiveCars = allCars.Count(c => c.IsAvailable);
            ViewBag.NewBookings = pendingBookings?.Count() ?? 0;
            ViewBag.PendingDocuments = pendingDocuments?.Count() ?? 0;
            
            // Для безопасности преобразуем в List<dynamic>
            ViewBag.RecentBookings = recentBookings?.Cast<dynamic>().ToList() ?? new List<dynamic>();
            ViewBag.RecentUsers = recentUsers?.Cast<dynamic>().ToList() ?? new List<dynamic>();
            
            return View();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка загрузки Dashboard: {ex.Message}");
            
            ViewBag.TotalUsers = 0;
            ViewBag.ActiveCars = 0;
            ViewBag.NewBookings = 0;
            ViewBag.PendingDocuments = 0;
            ViewBag.RecentBookings = new List<dynamic>();
            ViewBag.RecentUsers = new List<dynamic>();
            
            return View();
        }
    }
    
    [HttpGet("")]
    public IActionResult Index()
    {
        return RedirectToAction("Dashboard");
    }
}