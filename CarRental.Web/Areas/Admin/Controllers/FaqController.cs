using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Faq;
using CarRental.Web.ViewModels.Admin;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class FaqController : Controller
{
    private readonly IFaqService _faqService;
    private readonly ILogger<FaqController> _logger;

    public FaqController(IFaqService faqService, ILogger<FaqController> logger)
    {
        _faqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? category = null, bool? isActive = null, string? search = null)
    {
        ViewData["Title"] = "Управление FAQ";
        var allFaqs = await _faqService.GetAllFaqsAsync();
        var faqs = allFaqs.AsEnumerable();

        // ... фильтры

        var totalCount = faqs.Count();
        faqs = faqs.OrderBy(f => f.DisplayOrder).ThenByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize);

        // Получаем уникальные категории (не null или пустые)
        var categories = allFaqs
            .Select(f => f.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!) // т.к. мы отфильтровали null/empty, можно безопасно привести к не-null
            .Distinct()
            .ToList();

        var viewModel = new FaqIndexViewModel
        {
            Faqs = faqs.ToList(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            CategoryFilter = category,
            IsActiveFilter = isActive,
            Search = search,
            Categories = categories   // теперь это List<string>, а не List<string?>
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Создание вопроса";
        var model = new FaqCreateEditViewModel
        {
            IsActive = true,
            DisplayOrder = 0
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FaqCreateEditViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", viewModel);
        }

        try
        {
            var dto = new FaqCreateDto
            {
                Question = viewModel.Question,
                Answer = viewModel.Answer,
                Category = viewModel.Category,
                DisplayOrder = viewModel.DisplayOrder,
                IsActive = viewModel.IsActive
            };
            await _faqService.CreateFaqAsync(dto);
            TempData["SuccessMessage"] = "Вопрос успешно создан";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании FAQ");
            TempData["ErrorMessage"] = "Ошибка при создании вопроса";
            return View("Form", viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        ViewData["Title"] = "Редактирование вопроса";
        var faq = await _faqService.GetFaqByIdAsync(id);
        if (faq == null)
        {
            TempData["ErrorMessage"] = "Вопрос не найден";
            return RedirectToAction(nameof(Index));
        }

        var model = new FaqCreateEditViewModel
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
            Category = faq.Category,
            DisplayOrder = faq.DisplayOrder,
            IsActive = faq.IsActive
        };
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FaqCreateEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", viewModel);
        }

        try
        {
            var dto = new FaqUpdateDto
            {
                Id = viewModel.Id.Value,
                Question = viewModel.Question,
                Answer = viewModel.Answer,
                Category = viewModel.Category,
                DisplayOrder = viewModel.DisplayOrder,
                IsActive = viewModel.IsActive
            };
            await _faqService.UpdateFaqAsync(dto);
            TempData["SuccessMessage"] = "Вопрос успешно обновлён";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении FAQ");
            TempData["ErrorMessage"] = "Ошибка при обновлении вопроса";
            return View("Form", viewModel);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _faqService.DeleteFaqAsync(id);
            if (result)
                return Json(new { success = true, message = "Вопрос удалён" });
            else
                return Json(new { success = false, message = "Вопрос не найден" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении FAQ");
            return Json(new { success = false, message = "Ошибка при удалении" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            var isActive = await _faqService.ToggleFaqStatusAsync(id);
            return Json(new { success = true, isActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении статуса FAQ");
            return Json(new { success = false, message = "Ошибка при изменении статуса" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Reorder([FromBody] Dictionary<Guid, int> orders)
    {
        try
        {
            await _faqService.ReorderFaqsAsync(orders);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сортировке FAQ");
            return Json(new { success = false, message = "Ошибка при сортировке" });
        }
    }
}