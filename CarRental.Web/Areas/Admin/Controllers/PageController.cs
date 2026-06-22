using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Page;
using CarRental.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PageController : Controller
{
    private readonly IPageService _pageService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PageController> _logger;

    public PageController(IPageService pageService, IFileStorageService fileStorageService, ILogger<PageController> logger)
    {
        _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        _fileStorageService = fileStorageService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, bool? isActive = null, string? search = null)
    {
        ViewData["Title"] = "Управление страницами";

        var allPages = await _pageService.GetAllPagesAsync();
        var pages = allPages.AsEnumerable();

        if (isActive.HasValue)
            pages = pages.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            pages = pages.Where(p => p.Title.ToLower().Contains(searchLower) || p.Slug.ToLower().Contains(searchLower));
        }

        var totalCount = pages.Count();
        pages = pages.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Title)
            .Skip((page - 1) * pageSize).Take(pageSize);

        var viewModel = new PageIndexViewModel
        {
            Pages = pages.ToList(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            IsActiveFilter = isActive,
            Search = search
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Создание страницы";
        var model = new PageCreateEditViewModel
        {
            IsActive = true,
            DisplayOrder = 0,
            PublishedAt = DateTime.Today
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageCreateEditViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View("Form", viewModel);
        if (!await _pageService.IsSlugUniqueAsync(viewModel.Slug))
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "Страница с таким URL уже существует.");
            return View("Form", viewModel);
        }
        try
        {
            var dto = new PageCreateDto
            {
                Title = viewModel.Title,
                Slug = viewModel.Slug,
                Content = viewModel.Content,
                MetaDescription = viewModel.MetaDescription,
                IsActive = viewModel.IsActive,
                DisplayOrder = viewModel.DisplayOrder,
                PublishedAt = viewModel.PublishedAt?.ToUniversalTime()
            };

            await _pageService.CreatePageAsync(dto);
            TempData["SuccessMessage"] = "Страница успешно создана";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании страницы");
            TempData["ErrorMessage"] = "Ошибка при создании страницы";
            return View("Form", viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        ViewData["Title"] = "Редактирование страницы";
        var page = await _pageService.GetPageByIdAsync(id);
        if (page == null)
        {
            TempData["ErrorMessage"] = "Страница не найдена";
            return RedirectToAction(nameof(Index));
        }

        var model = new PageCreateEditViewModel
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Content = page.Content,
            MetaDescription = page.MetaDescription,
            IsActive = page.IsActive,
            DisplayOrder = page.DisplayOrder,
            PublishedAt = page.PublishedAt
        };

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PageCreateEditViewModel viewModel)
    {
        if (id != viewModel.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View("Form", viewModel);

        if (!await _pageService.IsSlugUniqueAsync(viewModel.Slug, id))
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "Страница с таким URL уже существует.");
            return View("Form", viewModel);
        }

        try
        {
            var dto = new PageUpdateDto
            {
                Id = viewModel.Id.Value,
                Title = viewModel.Title,
                Slug = viewModel.Slug,
                Content = viewModel.Content,
                MetaDescription = viewModel.MetaDescription,
                IsActive = viewModel.IsActive,
                DisplayOrder = viewModel.DisplayOrder,
                PublishedAt = viewModel.PublishedAt?.ToUniversalTime()
            };

            await _pageService.UpdatePageAsync(dto);
            TempData["SuccessMessage"] = "Страница успешно обновлена";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении страницы");
            TempData["ErrorMessage"] = "Ошибка при обновлении страницы";
            return View("Form", viewModel);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _pageService.DeletePageAsync(id);
            return Json(new { success = result, message = result ? "Страница удалена" : "Страница не найдена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении страницы");
            return Json(new { success = false, message = "Ошибка при удалении" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            var isActive = await _pageService.TogglePageStatusAsync(id);
            return Json(new { success = true, isActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении статуса страницы");
            return Json(new { success = false, message = "Ошибка при изменении статуса" });
        }
    }
    [Route("~/Admin/Page/upload-image")]
    [Authorize(Roles = "Admin")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        _logger.LogInformation("UploadImage called. File: {FileName}", file?.FileName);
        try
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Файл UploadImageне выбран" });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return Json(new { success = false, message = "Недопустимый формат" });

            var fileUrl = await _fileStorageService.SaveFileAsync(file, "pages");
            return Json(new { location = fileUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки изображения");
            return Json(new { success = false, message = ex.Message });
        }
    }
}