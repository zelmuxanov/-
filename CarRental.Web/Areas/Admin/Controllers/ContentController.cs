using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Enums;
using CarRental.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using CarRental.BLL.DTOs.Banner;

namespace CarRental.Web.Areas.Admin.Controllers;

public class ContentController : BaseAdminController
{
    private readonly IBannerService _bannerService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ContentController> _logger;

    public ContentController(
        IBannerService bannerService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<ContentController> logger)
    {
        _bannerService = bannerService ?? throw new ArgumentNullException(nameof(bannerService));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Управление контентом";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Banners(
        int page = 1,
        int pageSize = 10,
        BannerType? typeFilter = null,
        bool? isActiveFilter = null,
        string? search = null)
    {
        try
        {
            ViewData["Title"] = "Управление баннерами";
            
            var allBanners = await _bannerService.GetAllBannersAsync();
            var banners = allBanners.AsEnumerable();
            
            // Применяем фильтры
            if (typeFilter.HasValue)
            {
                banners = banners.Where(b => b.BannerType == typeFilter.Value);
            }
            
            if (isActiveFilter.HasValue)
            {
                banners = banners.Where(b => b.IsActive == isActiveFilter.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                banners = banners.Where(b => 
                    (b.Title != null && b.Title.ToLower().Contains(searchLower)) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchLower)));
            }
            
            // Пагинация
            var totalCount = banners.Count();
            banners = banners
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            
            var viewModel = new BannerIndexViewModel
            {
                Banners = banners,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TypeFilter = typeFilter,
                IsActiveFilter = isActiveFilter,
                Search = search
            };
            
            ViewBag.BannerTypes = GetBannerTypesSelectList(null);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке списка баннеров");
            TempData["ErrorMessage"] = "Ошибка при загрузке баннеров";
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Создание баннера";
        
        var viewModel = new BannerCreateEditViewModel
        {
            BannerTypes = GetBannerTypesSelectList(BannerType.MainCarousel),
            IsActive = true,
            DisplayOrder = 0,
            ButtonText = "Подробнее"
        };
        
        return View("BannerForm", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BannerCreateEditViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Ошибки валидации: {Errors}", errors);
                viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
                return View("BannerForm", viewModel);
            }

            string mediaUrl = string.Empty;

            if (viewModel.MediaType == BannerMediaType.Image)
            {
                if (viewModel.ImageFile == null || viewModel.ImageFile.Length == 0)
                {
                    ModelState.AddModelError("ImageFile", "Требуется загрузить изображение");
                    viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
                    return View("BannerForm", viewModel);
                }
                mediaUrl = await ProcessUploadedFileAsync(viewModel.ImageFile);
            }
            else // Video
            {
                if (viewModel.VideoFile != null && viewModel.VideoFile.Length > 0)
                {
                    mediaUrl = await ProcessUploadedVideoAsync(viewModel.VideoFile);
                }
                else if (!string.IsNullOrWhiteSpace(viewModel.VideoUrl))
                {
                    // валидация ссылки (можно дополнительно проверить, что это YouTube/Vimeo или просто url)
                    mediaUrl = viewModel.VideoUrl;
                }
                else
                {
                    ModelState.AddModelError("VideoUrl", "Требуется загрузить видео или указать ссылку");
                    viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
                    return View("BannerForm", viewModel);
                }
            }

            var bannerDto = new BannerCreateDto
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                ImageUrl = viewModel.MediaType == BannerMediaType.Image ? mediaUrl : string.Empty,
                VideoUrl = viewModel.MediaType == BannerMediaType.Video ? mediaUrl : null,
                MediaType = viewModel.MediaType,
                Link = viewModel.Link,
                DisplayOrder = viewModel.DisplayOrder,
                IsActive = viewModel.IsActive,
                StartDate = viewModel.StartDate?.ToUniversalTime(),
                EndDate = viewModel.EndDate?.ToUniversalTime(),
                ButtonText = viewModel.ButtonText,
                BannerType = viewModel.BannerType,
                VideoAutoplay = viewModel.VideoAutoplay,
                VideoMuted = viewModel.VideoMuted,
                VideoLoop = viewModel.VideoLoop,
                ObjectFit = viewModel.ObjectFit,
                ObjectPosition = viewModel.ObjectPosition,
                VideoControls = viewModel.VideoControls

            };

            await _bannerService.CreateBannerAsync(bannerDto);

            TempData["SuccessMessage"] = "Баннер успешно создан";
            return RedirectToAction("Banners");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании баннера");
            TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
            return View("BannerForm", viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            ViewData["Title"] = "Редактирование баннера";
            
            var bannerDto = await _bannerService.GetBannerByIdAsync(id);
            if (bannerDto == null)
            {
                TempData["ErrorMessage"] = "Баннер не найден";
                return RedirectToAction("Banners");
            }
            
            var viewModel = new BannerCreateEditViewModel
            {
                Id = bannerDto.Id,
                Title = bannerDto.Title,
                Description = bannerDto.Description,
                CurrentImageUrl = bannerDto.ImageUrl,
                CurrentVideoUrl = bannerDto.VideoUrl,           // <-- добавить
                Link = bannerDto.Link,
                DisplayOrder = bannerDto.DisplayOrder,
                IsActive = bannerDto.IsActive,
                StartDate = bannerDto.StartDate,
                EndDate = bannerDto.EndDate,
                ButtonText = bannerDto.ButtonText,
                BannerType = bannerDto.BannerType,
                BannerTypes = GetBannerTypesSelectList(bannerDto.BannerType),
                MediaType = bannerDto.MediaType,                // <-- добавить
                VideoAutoplay = bannerDto.VideoAutoplay,        // <-- добавить
                VideoMuted = bannerDto.VideoMuted,              // <-- добавить
                VideoLoop = bannerDto.VideoLoop,                // <-- добавить
                ObjectFit = bannerDto.ObjectFit ?? "cover",
                ObjectPosition = bannerDto.ObjectPosition ?? "center",
                VideoControls = bannerDto.VideoControls         // <-- добавить
            };
            
            return View("BannerForm", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке баннера для редактирования");
            TempData["ErrorMessage"] = "Ошибка при загрузке баннера";
            return RedirectToAction("Banners");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, BannerCreateEditViewModel viewModel)
    {
        try
        {
            _logger.LogInformation("Редактирование баннера {BannerId}", id);

            // Проверяем существование баннера
            var existingBanner = await _bannerService.GetBannerByIdAsync(id);
            if (existingBanner == null)
            {
                TempData["ErrorMessage"] = "Баннер не найден";
                return RedirectToAction("Banners");
            }

            // Валидация модели
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Ошибки валидации: {Errors}", errors);
                _logger.LogWarning("Модель невалидна при редактировании баннера {BannerId}", id);
                viewModel.CurrentImageUrl = existingBanner.ImageUrl;
                viewModel.CurrentVideoUrl = existingBanner.VideoUrl;
                viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
                viewModel.Id = id;
                return View("BannerForm", viewModel);
            }

            // Определяем, какой тип медиа выбран и какой был ранее
            var oldMediaType = existingBanner.MediaType;
            var newMediaType = viewModel.MediaType;

            string mediaUrl = string.Empty;

            // Обработка в зависимости от нового типа
            if (newMediaType == BannerMediaType.Image)
            {
                // Если загружено новое изображение
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    mediaUrl = await ProcessUploadedFileAsync(viewModel.ImageFile);
                    
                    // Удаляем старое изображение (если оно было)
                    if (!string.IsNullOrEmpty(existingBanner.ImageUrl))
                    {
                        await DeleteFileAsync(existingBanner.ImageUrl);
                    }
                    
                    // Если старый тип был видео – удаляем видеофайл
                    if (oldMediaType == BannerMediaType.Video && !string.IsNullOrEmpty(existingBanner.VideoUrl))
                    {
                        await DeleteFileAsync(existingBanner.VideoUrl);
                    }
                }
                else
                {
                    // Оставляем старое изображение
                    mediaUrl = existingBanner.ImageUrl;
                    
                    // Если тип сменился с видео на изображение, но новое изображение не загружено,
                    // используем старое изображение (если оно было). При этом удаляем видео, если оно было.
                    if (oldMediaType == BannerMediaType.Video && !string.IsNullOrEmpty(existingBanner.VideoUrl))
                    {
                        await DeleteFileAsync(existingBanner.VideoUrl);
                    }
                }
            }
            else // Video
            {
                // Приоритет: загруженный файл > указанная ссылка > существующее видео (если тип не менялся)
                if (viewModel.VideoFile != null && viewModel.VideoFile.Length > 0)
                {
                    mediaUrl = await ProcessUploadedVideoAsync(viewModel.VideoFile);
                    
                    // Удаляем старое видео, если оно было
                    if (oldMediaType == BannerMediaType.Video && !string.IsNullOrEmpty(existingBanner.VideoUrl))
                    {
                        await DeleteFileAsync(existingBanner.VideoUrl);
                    }
                    
                    // Если старый тип был изображение – удаляем старое изображение
                    if (oldMediaType == BannerMediaType.Image && !string.IsNullOrEmpty(existingBanner.ImageUrl))
                    {
                        await DeleteFileAsync(existingBanner.ImageUrl);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(viewModel.VideoUrl))
                {
                    mediaUrl = viewModel.VideoUrl;
                    
                    // Удаляем старый файл, если был загруженный
                    if (oldMediaType == BannerMediaType.Video && !string.IsNullOrEmpty(existingBanner.VideoUrl) 
                        && existingBanner.VideoUrl.StartsWith("/uploads/"))
                    {
                        await DeleteFileAsync(existingBanner.VideoUrl);
                    }
                    
                    if (oldMediaType == BannerMediaType.Image && !string.IsNullOrEmpty(existingBanner.ImageUrl))
                    {
                        await DeleteFileAsync(existingBanner.ImageUrl);
                    }
                }
                else
                {
                    // Если тип не менялся и оставляем старое видео (ссылку или файл)
                    if (oldMediaType == BannerMediaType.Video && !string.IsNullOrEmpty(existingBanner.VideoUrl))
                    {
                        mediaUrl = existingBanner.VideoUrl;
                    }
                    else
                    {
                        ModelState.AddModelError("VideoUrl", "Требуется загрузить видео или указать ссылку");
                        viewModel.CurrentImageUrl = existingBanner.ImageUrl;
                        viewModel.CurrentVideoUrl = existingBanner.VideoUrl;
                        viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
                        viewModel.Id = id;
                        return View("BannerForm", viewModel);
                    }
                }
            }

            // Формируем DTO для обновления
            var bannerUpdateDto = new BannerUpdateDto
            {
                Id = id,
                Title = viewModel.Title,
                Description = viewModel.Description,
                // В зависимости от типа медиа заполняем соответствующие поля
                ImageUrl = newMediaType == BannerMediaType.Image ? mediaUrl : string.Empty,
                VideoUrl = newMediaType == BannerMediaType.Video ? mediaUrl : null,
                MediaType = newMediaType,
                Link = viewModel.Link,
                DisplayOrder = viewModel.DisplayOrder,
                IsActive = viewModel.IsActive,
                StartDate = viewModel.StartDate?.ToUniversalTime(),
                EndDate = viewModel.EndDate?.ToUniversalTime(),
                ButtonText = viewModel.ButtonText,
                BannerType = viewModel.BannerType,
                VideoAutoplay = viewModel.VideoAutoplay,
                VideoMuted = viewModel.VideoMuted,
                VideoLoop = viewModel.VideoLoop,
                ObjectFit = viewModel.ObjectFit,
                ObjectPosition = viewModel.ObjectPosition,
                VideoControls = viewModel.VideoControls
            };

            await _bannerService.UpdateBannerAsync(bannerUpdateDto);

            TempData["SuccessMessage"] = "Баннер успешно обновлен";
            _logger.LogInformation("Баннер {BannerId} успешно обновлен", id);

            return RedirectToAction("Banners");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении баннера {BannerId}", id);
            TempData["ErrorMessage"] = $"Ошибка при обновлении баннера: {ex.Message}";

            // Восстанавливаем данные для отображения формы
            var existingBanner = await _bannerService.GetBannerByIdAsync(id);
            viewModel.CurrentImageUrl = existingBanner?.ImageUrl;
            viewModel.CurrentVideoUrl = existingBanner?.VideoUrl;
            viewModel.BannerTypes = GetBannerTypesSelectList(viewModel.BannerType);
            viewModel.Id = id;
            return View("BannerForm", viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            // Получаем баннер, чтобы удалить связанные файлы
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner != null)
            {
                // Удаляем изображение, если оно локальное
                if (!string.IsNullOrEmpty(banner.ImageUrl) && banner.ImageUrl.StartsWith("/uploads/"))
                {
                    await DeleteFileAsync(banner.ImageUrl);
                }

                // Удаляем видео, если оно локальное
                if (!string.IsNullOrEmpty(banner.VideoUrl) && banner.VideoUrl.StartsWith("/uploads/"))
                {
                    await DeleteFileAsync(banner.VideoUrl);
                }
            }

            var result = await _bannerService.DeleteBannerAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Баннер успешно удален";
            }
            else
            {
                TempData["ErrorMessage"] = "Баннер не найден";
            }
            return RedirectToAction("Banners");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении баннера");
            TempData["ErrorMessage"] = "Ошибка при удалении баннера";
            return RedirectToAction("Banners");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            var result = await _bannerService.ToggleBannerStatusAsync(id);
            
            return Json(new { success = true, isActive = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении статуса баннера");
            return Json(new { success = false, message = "Ошибка при изменении статуса" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Reorder([FromBody] Dictionary<Guid, int> bannerOrders)
    {
        try
        {
            if (bannerOrders == null || !bannerOrders.Any())
                return Json(new { success = false, message = "Неверные данные" });
            
            await _bannerService.ReorderBannersAsync(bannerOrders);
            
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении порядка баннеров");
            return Json(new { success = false, message = "Ошибка при изменении порядка" });
        }
    }

    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

    private async Task<string> ProcessUploadedFileAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return string.Empty;
            
        // Проверка расширения файла
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"Недопустимый формат файла. Разрешены: {string.Join(", ", allowedExtensions)}");
        }
        
        // Проверка размера файла (5MB)
        const long maxSize = 5 * 1024 * 1024;
        if (file.Length > maxSize)
        {
            throw new ArgumentException($"Файл слишком большой. Максимальный размер: {maxSize / (1024 * 1024)}MB");
        }
        
        // Создание уникального имени файла
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "banners");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);
        
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        
        // Сохранение файла
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        
        // Возвращаем относительный URL
        return $"/uploads/banners/{uniqueFileName}";
    }
    
    private Task DeleteFileAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return Task.CompletedTask;
                
            // Из относительного URL получаем физический путь
            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
            
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                _logger.LogInformation("Файл удален: {FilePath}", filePath);
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении файла: {ImageUrl}", imageUrl);
            return Task.CompletedTask;
        }
    }
    
    private List<SelectListItem> GetBannerTypesSelectList(BannerType? selectedType)
    {
        return Enum.GetValues<BannerType>()
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = GetBannerTypeDisplayName(t),
                Selected = t == selectedType
            })
            .ToList();
    }
    
    private string GetBannerTypeDisplayName(BannerType bannerType)
    {
        return bannerType switch
        {
            BannerType.MainCarousel => "Главная карусель",
            BannerType.Sidebar => "Боковой баннер",
            BannerType.Promotion => "Акционный баннер",
            BannerType.Information => "Информационный блок",
            _ => bannerType.ToString()
        };
    }
    private async Task<string> ProcessUploadedVideoAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        var allowedExtensions = new[] { ".mp4", ".webm", ".ogv", ".mov" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Недопустимый формат видео. Разрешены: {string.Join(", ", allowedExtensions)}");

        const long maxSize = 100 * 1024 * 1024; // 100 MB
        if (file.Length > maxSize)
            throw new ArgumentException($"Видео слишком большое. Максимальный размер: {maxSize / (1024 * 1024)}MB");

        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "banners", "videos");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/banners/videos/{uniqueFileName}";
    }
}