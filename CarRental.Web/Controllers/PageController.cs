using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Controllers;

[Route("page")]
public class PageController : Controller
{
    private readonly IPageService _pageService;
    private readonly ILogger<PageController> _logger;

    public PageController(IPageService pageService, ILogger<PageController> logger)
    {
        _pageService = pageService;
        _logger = logger;
    }

    [HttpGet("menu")]
    public async Task<IActionResult> Menu()
    {
        try
        {
            var pages = await _pageService.GetActivePagesAsync();
            var menuItems = pages
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new { p.Title, p.Slug })
                .ToList();
            return Json(menuItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки страниц меню");
            return Json(new List<object>());
        }
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Display(string slug)
    {
        var page = await _pageService.GetPageBySlugAsync(slug);
        if (page == null)
            return NotFound();

        return View("View", page);
    }
}