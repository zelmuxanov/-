using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRental.Domain.Enums;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class BaseAdminController : Controller
{
    // УДАЛИТЕ все переопределения View - они ломают стандартный механизм Areas!
    // Вместо этого используйте стандартный механизм ASP.NET Core
    
    protected void AddModelError(string key, string message)
    {
        ModelState.AddModelError(key, message);
    }
    
    protected bool ValidateBookingDates(DateTime startDate, DateTime endDate)
    {
        var rentalDays = (endDate - startDate).Days;
        return rentalDays >= 3;
    }
    
    protected void LogError(Exception ex, string message)
    {
        Console.WriteLine($"ERROR: {message} - {ex.Message}");
    }
    
    // Добавьте вспомогательные методы для работы с Area
    
    protected string GetAreaName()
    {
        return "Admin";
    }
    
    protected IActionResult RedirectToAreaAction(string action, string controller)
    {
        return RedirectToAction(action, controller, new { area = "Admin" });
    }
    
    protected IActionResult RedirectToAreaAction(string action)
    {
        var controllerName = GetType().Name.Replace("Controller", "");
        return RedirectToAction(action, controllerName, new { area = "Admin" });
    }
}