using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Car;
using CarRental.Web.ViewModels.Admin;
using CarRental.Domain.Enums;
using CarRental.Web.Areas.Admin;

namespace CarRental.Web.Areas.Admin.Controllers;

[Route("Admin/Cars")]
public class CarsController : BaseAdminController  
{
    private readonly ICarService _carService;
    private readonly ILogger<CarsController> _logger;
    private readonly IBookingService _bookingService;

    public CarsController(
        ICarService carService,
        ILogger<CarsController> logger,
        IBookingService bookingService)
    {
        _carService = carService;
        _logger = logger;
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewData["Title"] = "Управление автомобилями";
            var cars = await _carService.GetAllCarsAsync();
            
            // Диагностика: выводим путь к View
            var controllerName = this.GetType().Name.Replace("Controller", "");
            Console.WriteLine($"Ищу View для {controllerName}/Index.cshtml");
            
            return View(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке автомобилей");
            TempData["ErrorMessage"] = "Ошибка при загрузке автомобилей";
            return View(new List<CarDto>());
        }
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            ViewData["Title"] = "Добавить автомобиль";
            
            var viewModel = new CarCreateViewModel
            {
                Year = DateTime.Now.Year,
                Seats = 5,
                IsAvailable = true,
                EngineCapacityString = "2.0",
                Description = "",
                Class = CarClass.Standard,
                FuelType = FuelType.Petrol,
                Transmission = TransmissionType.Automatic,
                ExistingBrands = await _carService.GetUniqueBrandsAsync() ?? new List<string>(),
                ExistingModels = await _carService.GetUniqueModelsAsync() ?? new List<string>()
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке формы создания автомобиля");
            TempData["ErrorMessage"] = "Ошибка при загрузке формы";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CarCreateViewModel? viewModel)
    {
        // Проверяем, что viewModel не null
        if (viewModel == null)
        {
            Console.WriteLine("=== ERROR: viewModel IS NULL ===");
            TempData["ErrorMessage"] = "Данные формы не получены";
            
            // Восстанавливаем списки
            var newViewModel = new CarCreateViewModel
            {
                ExistingBrands = await _carService.GetUniqueBrandsAsync() ?? new List<string>(),
                ExistingModels = await _carService.GetUniqueModelsAsync() ?? new List<string>()
            };
            
            return View(newViewModel);
        }

        try
        {
            Console.WriteLine("=== СОЗДАНИЕ АВТОМОБИЛЯ ===");
            
            // ДИАГНОСТИКА: Читаем сырые данные запроса
            Console.WriteLine("=== RAW REQUEST DATA ===");
            
            // 1. Показываем все параметры запроса
            if (Request.HasFormContentType)
            {
                Console.WriteLine("Form data:");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"  {key}: {Request.Form[key]}");
                }
            }
            
            
            // Проверяем валидацию
            if (ModelState != null && !ModelState.IsValid)
            {
                Console.WriteLine("❌ Ошибки валидации ModelState:");
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (entry != null && entry.Errors.Count > 0)
                    {
                        Console.WriteLine($"  {key}:");
                        foreach (var error in entry.Errors)
                        {
                            Console.WriteLine($"    - {error.ErrorMessage}");
                        }
                    }
                }
                
                // Восстанавливаем списки
                viewModel.ExistingBrands = await _carService.GetUniqueBrandsAsync() ?? new List<string>();
                viewModel.ExistingModels = await _carService.GetUniqueModelsAsync() ?? new List<string>();
                return View(viewModel);
            }

            // Конвертируем ViewModel в CarCreateDto
            var createDto = new CarCreateDto
            {
                Brand = viewModel.Brand,
                Model = viewModel.Model,
                Year = viewModel.Year,
                Color = viewModel.Color,
                PricePerDay = viewModel.PricePerDay,
                IsAvailable = viewModel.IsAvailable,
                Description = viewModel.Description,
                Class = viewModel.Class ?? CarClass.Standard,
                Transmission = viewModel.Transmission ?? TransmissionType.Automatic,
                FuelType = viewModel.FuelType ?? FuelType.Petrol,
                Seats = viewModel.Seats,
                EngineCapacity = viewModel.EngineCapacity,
                LicensePlate = viewModel.LicensePlate,
                VIN = viewModel.VIN,
                ImageFiles = viewModel.ImageFiles,
                PricePerDay15 = viewModel.PricePerDay15,
                PricePerDay30 = viewModel.PricePerDay30,
                Deposit = viewModel.Deposit,
                MileageLimitPerDay = viewModel.MileageLimitPerDay,
                OverMileagePricePerKm = viewModel.OverMileagePricePerKm,
                UnlimitedMileagePrice = viewModel.UnlimitedMileagePrice
            };

            var result = await _carService.CreateCarAsync(createDto);
            
            if (result != null)
            {
                TempData["SuccessMessage"] = $"Автомобиль {result.Brand} {result.Model} успешно добавлен";
                Console.WriteLine($"✅ Автомобиль создан: {result.Id}");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось добавить автомобиль";
                Console.WriteLine("❌ Результат создания автомобиля - null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании автомобиля");
            TempData["ErrorMessage"] = $"Ошибка при создании: {ex.Message}";
            Console.WriteLine($"💥 Ошибка: {ex.Message}");
            Console.WriteLine($"💥 StackTrace: {ex.StackTrace}");
            
            // Восстанавливаем списки
            viewModel.ExistingBrands = await _carService.GetUniqueBrandsAsync() ?? new List<string>();
            viewModel.ExistingModels = await _carService.GetUniqueModelsAsync() ?? new List<string>();
        }
        
        return View(viewModel);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            ViewData["Title"] = "Редактировать автомобиль";
            
            // Важно: получаем автомобиль с подгруженными изображениями
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
            {
                TempData["ErrorMessage"] = "Автомобиль не найден";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CarEditViewModel
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Color = car.Color,
                PricePerDay = car.PricePerDay,
                IsAvailable = car.IsAvailable,
                Description = car.Description ?? "",
                Class = car.Class,
                Transmission = car.Transmission,
                FuelType = car.FuelType,
                Seats = car.Seats,
                EngineCapacityString = car.EngineCapacity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                // Заполняем существующие изображения
                LicensePlate = car.LicensePlate,
                VIN = car.VIN,
                ExistingImages = car.Images ?? new List<CarImageDto>(),
                PricePerDay15 = car.PricePerDay15,
                PricePerDay30 = car.PricePerDay30,
                Deposit = car.Deposit,
                MileageLimitPerDay = car.MileageLimitPerDay,
                OverMileagePricePerKm = car.OverMileagePricePerKm,
                UnlimitedMileagePrice = car.UnlimitedMileagePrice
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке автомобиля {CarId} для редактирования", id);
            TempData["ErrorMessage"] = "Ошибка при загрузке автомобиля";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CarEditViewModel viewModel)
    {
        try
        {
            Console.WriteLine($"=== РЕДАКТИРОВАНИЕ АВТОМОБИЛЯ {id} ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState?.IsValid}");
            
            // Проверяем, что viewModel не null
            if (viewModel == null)
            {
                Console.WriteLine("=== ERROR: viewModel IS NULL ===");
                TempData["ErrorMessage"] = "Данные формы не получены";
                return RedirectToAction(nameof(Edit), new { id });
            }

            // ДИАГНОСТИКА: Читаем сырые данные запроса
            Console.WriteLine("=== RAW REQUEST DATA ===");
            
            if (Request.HasFormContentType)
            {
                Console.WriteLine("Form data:");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"  {key}: {Request.Form[key]}");
                }
            }
            
            Console.WriteLine("=== MODEL AFTER BINDING ===");
            Console.WriteLine($"Brand: '{viewModel.Brand}'");
            Console.WriteLine($"Model: '{viewModel.Model}'");
            Console.WriteLine($"Year: {viewModel.Year}");
            Console.WriteLine($"Class: {viewModel.Class}");
            Console.WriteLine($"FuelType: {viewModel.FuelType}");
            Console.WriteLine($"Transmission: {viewModel.Transmission}");
            Console.WriteLine($"PricePerDay: {viewModel.PricePerDay}");
            Console.WriteLine($"Seats: {viewModel.Seats}");
            Console.WriteLine($"EngineCapacityString: '{viewModel.EngineCapacityString}'");
            Console.WriteLine($"EngineCapacity (parsed): {viewModel.EngineCapacity}");
            Console.WriteLine($"IsAvailable: {viewModel.IsAvailable}");
            
            // Проверяем валидацию
            if (ModelState != null && !ModelState.IsValid)
            {
                Console.WriteLine("❌ Ошибки валидации ModelState:");
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (entry != null && entry.Errors.Count > 0)
                    {
                        Console.WriteLine($"  {key}:");
                        foreach (var error in entry.Errors)
                        {
                            Console.WriteLine($"    - {error.ErrorMessage}");
                        }
                    }
                }
                
                // Восстанавливаем текущее изображение
                var car = await _carService.GetCarByIdAsync(id);
                if (car != null)
                {
                    viewModel.CurrentImageUrl = car.MainImageUrl;
                }
                
                return View(viewModel);
            }

            Console.WriteLine($"✅ Данные автомобиля (валидны):");
            Console.WriteLine($"  Brand: {viewModel.Brand}");
            Console.WriteLine($"  Model: {viewModel.Model}");
            Console.WriteLine($"  Year: {viewModel.Year}");
            Console.WriteLine($"  Class: {viewModel.Class}");
            Console.WriteLine($"  FuelType: {viewModel.FuelType}");
            Console.WriteLine($"  Transmission: {viewModel.Transmission}");
            Console.WriteLine($"  Price: {viewModel.PricePerDay}");
            Console.WriteLine($"  Seats: {viewModel.Seats}");
            Console.WriteLine($"  EngineCapacityString: {viewModel.EngineCapacityString}");
            Console.WriteLine($"  EngineCapacity (parsed): {viewModel.EngineCapacity}");
            Console.WriteLine($"  IsAvailable: {viewModel.IsAvailable}");

            // Конвертируем ViewModel в CarUpdateDto
            var updateDto = new CarUpdateDto
            {
                Brand = viewModel.Brand,
                Model = viewModel.Model,
                Year = viewModel.Year,
                Color = viewModel.Color,
                PricePerDay = viewModel.PricePerDay,
                IsAvailable = viewModel.IsAvailable,
                Description = viewModel.Description,
                Class = viewModel.Class ?? CarClass.Standard,
                Transmission = viewModel.Transmission ?? TransmissionType.Automatic,
                FuelType = viewModel.FuelType ?? FuelType.Petrol,
                Seats = viewModel.Seats,
                EngineCapacity = viewModel.EngineCapacity,
                LicensePlate = viewModel.LicensePlate,
                VIN = viewModel.VIN,
                ImageFiles = viewModel.ImageFiles,
                PricePerDay15 = viewModel.PricePerDay15,
                PricePerDay30 = viewModel.PricePerDay30,
                Deposit = viewModel.Deposit,
                MileageLimitPerDay = viewModel.MileageLimitPerDay,
                OverMileagePricePerKm = viewModel.OverMileagePricePerKm,
                UnlimitedMileagePrice = viewModel.UnlimitedMileagePrice
            };

            var result = await _carService.UpdateCarAsync(id, updateDto);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Автомобиль успешно обновлен";
                Console.WriteLine($"✅ Автомобиль обновлен: {id}");
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось обновить автомобиль";
                Console.WriteLine($"❌ Не удалось обновить автомобиль: {id}");
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении автомобиля {CarId}", id);
            TempData["ErrorMessage"] = $"Ошибка при обновлении: {ex.Message}";
            Console.WriteLine($"💥 Ошибка: {ex.Message}");
            Console.WriteLine($"💥 StackTrace: {ex.StackTrace}");
            
            // Восстанавливаем текущее изображение
            var car = await _carService.GetCarByIdAsync(id);
            if (car != null)
            {
                viewModel.CurrentImageUrl = car.MainImageUrl;
            }
            
            return View(viewModel);
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            ViewData["Title"] = "Детали автомобиля";
            
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
            {
                TempData["ErrorMessage"] = "Автомобиль не найден";
                return RedirectToAction(nameof(Index));
            }
            
            return View(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке автомобиля {CarId}", id);
            TempData["ErrorMessage"] = "Ошибка при загрузке автомобиля";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _carService.DeleteCarAsync(id);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Автомобиль успешно удален";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось удалить автомобиль";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении автомобиля {CarId}", id);
            TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleAvailability/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        try
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
            {
                TempData["ErrorMessage"] = "Автомобиль не найден";
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new CarUpdateDto
            {
                IsAvailable = !car.IsAvailable
            };

            var result = await _carService.UpdateCarAsync(id, updateDto);
            
            if (result)
            {
                TempData["SuccessMessage"] = $"Статус доступности изменен на {(updateDto.IsAvailable == true ? "Доступен" : "Недоступен")}";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось изменить статус доступности";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении статуса автомобиля {CarId}", id);
            TempData["ErrorMessage"] = $"Ошибка при изменении статуса: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    // ... остальные методы без изменений ...
    [HttpPost("upload-images-temp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImagesTemp(List<IFormFile> files)
    {
        try
        {
            if (files == null || !files.Any())
                return Json(new { success = false, message = "Файлы не выбраны" });

            var images = new List<object>();
            foreach (var file in files)
            {
                var url = await _carService.UploadTempImageAsync(file);
                images.Add(new { id = Guid.NewGuid(), imageUrl = url, isMain = false });
            }
            return Json(new { success = true, images });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка временной загрузки");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("delete-image/{imageId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        var result = await _carService.DeleteCarImageAsync(imageId);
        return Json(new { success = result });
    }

    [HttpPost("set-main-image/{imageId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMainImage(Guid imageId)
    {
        var result = await _carService.SetMainImageAsync(imageId);
        return Json(new { success = result });
    }
    [HttpPost("reorder-images")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderImages([FromBody] ReorderRequest request)
    {
        if (request?.OrderedIds == null) return Json(new { success = false });
        var result = await _carService.ReorderCarImagesAsync(request.OrderedIds);
        return Json(new { success = result });
    }

    public class ReorderRequest
    {
        public List<Guid> OrderedIds { get; set; } = new();
    }
    [HttpPost("update-image-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateImageOrder([FromBody] UpdateImageOrderRequest request)
    {
        if (request == null || request.ImageId == Guid.Empty)
            return Json(new { success = false });
        try
        {
            await _carService.UpdateImageOrderAsync(request.ImageId, request.NewOrder);
            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false });
        }
    }

    public class UpdateImageOrderRequest
    {
        public Guid ImageId { get; set; }
        public int NewOrder { get; set; }
    }
    [HttpPost("update-images-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateImagesOrder([FromBody] List<Guid> orderedImageIds)
    {
        if (orderedImageIds == null || !orderedImageIds.Any())
            return Json(new { success = false });
        var result = await _carService.UpdateImagesOrderAsync(orderedImageIds);
        return Json(new { success = result });
    }
    [HttpGet("GetBookingsForCar/{carId}")]
    public async Task<IActionResult> GetBookingsForCar(Guid carId)
    {
        var bookings = await _bookingService.GetBookingsForCarAsync(carId);
        var events = bookings.Select(b => new
        {
            id = b.Id,
            title = $"{b.User?.FirstName} {b.User?.LastName}",
            start = b.StartDate.ToString("yyyy-MM-dd"),
            end = b.EndDate.AddDays(1).ToString("yyyy-MM-dd"),
            color = b.Status == BookingStatus.Confirmed ? "#28a745" : (b.Status == BookingStatus.Pending ? "#ffc107" : "#6c757d"),
            extendedProps = new { notes = b.Notes, userId = b.UserId, status = b.Status.ToString() }
        });
        return Json(events);
    }
}