using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICarService _carService;
    private readonly IBannerService _bannerService;
    private readonly ILogger<HomeController> _logger;
    private readonly IFaqService _faqService;

    public HomeController(
        ICarService carService,
        IBannerService bannerService,
        IFaqService faqService,
        ILogger<HomeController> logger)
    {
        _carService = carService ?? throw new ArgumentNullException(nameof(carService));
        _bannerService = bannerService ?? throw new ArgumentNullException(nameof(bannerService));
        _faqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            ViewData["Title"] = "CarRental - Аренда автомобилей";
            
            // Получаем автомобили для блока "Популярные"
            var cars = await _carService.GetAllCarsAsync();
            var availableCars = cars.Where(c => c.IsAvailable).ToList();
            var popularCars = cars
                .Where(c => c.IsAvailable)
                .OrderByDescending(c => c.CreatedAt)
                .Take(6);

            ViewBag.TotalCars = availableCars.Count;
            
            // Получаем активные баннеры для карусели
            var banners = await _bannerService.GetBannersByTypeAsync(
                CarRental.Domain.Enums.BannerType.MainCarousel);
            
            ViewBag.Banners = banners;
            
            return View(popularCars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке главной страницы");
            ViewBag.Banners = new List<CarRental.BLL.DTOs.Banner.BannerDto>();
            return View(Enumerable.Empty<CarRental.BLL.DTOs.Car.CarDto>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public async Task<IActionResult> FAQ()
    {
        var faqs = await _faqService.GetActiveFaqsAsync();
        return View(faqs);
    }
    public IActionResult Terms()
    {
        return View();
    }
    public IActionResult Reviews()
    {
        return View();
    }
}