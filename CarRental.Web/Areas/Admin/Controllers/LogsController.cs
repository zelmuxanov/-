using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Logs")]
[Authorize(Roles = "Admin")]
public class LogsController : BaseAdminController
{
    private readonly ILogViewerService _logViewer;

    public LogsController(ILogViewerService logViewer)
    {
        _logViewer = logViewer;
    }

    [HttpGet]
    public IActionResult Index(string? level)
    {
        LogLevel? minLevel = level?.ToLower() switch
        {
            "error" => LogLevel.Error,
            "warning" => LogLevel.Warning,
            "info" => LogLevel.Information,
            _ => null
        };

        var logs = _logViewer.GetRecentLogs(100, minLevel);
        ViewBag.CurrentLevel = level;
        return View(logs);
    }
}