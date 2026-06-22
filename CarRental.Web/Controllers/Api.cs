using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Controllers.Api;

[Route("api/pages")]
[ApiController]
public class PagesApiController : ControllerBase
{
    private readonly IPageService _pageService;

    public PagesApiController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet("menu")]
    public async Task<IActionResult> GetMenuPages()
    {
        var pages = await _pageService.GetActivePagesAsync();
        var menuItems = pages
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new { title = p.Title, slug = p.Slug })
            .ToList();
        return Ok(menuItems);
    }
}