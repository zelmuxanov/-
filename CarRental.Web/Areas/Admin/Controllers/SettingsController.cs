using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Settings;
using CarRental.Web.ViewModels.Admin;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : BaseAdminController
{
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SettingsController(
        ISiteSettingsService siteSettingsService,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _siteSettingsService = siteSettingsService;
        _environment = environment;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("General")]
    public IActionResult General()
    {
        var settings = _siteSettingsService.GetSettings();
        var model = new GeneralSettingsViewModel
        {
            CompanyName = settings.CompanyName,
            Phone = settings.Phone,
            Email = settings.Email,
            Address = settings.Address,
            CurrentLogoUrl = settings.LogoUrl,
            WhatsAppUrl = settings.WhatsAppUrl,
            TelegramUrl = settings.TelegramUrl,
            MaxUrl = settings.MaxUrl
        };
        return View(model);
    }

    [HttpPost("General")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> General(GeneralSettingsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var settings = _siteSettingsService.GetSettings();
        settings.CompanyName = model.CompanyName;
        settings.Phone = model.Phone;
        settings.Email = model.Email;
        settings.Address = model.Address;
        settings.WhatsAppUrl = model.WhatsAppUrl;
        settings.TelegramUrl = model.TelegramUrl;
        settings.MaxUrl = model.MaxUrl;

        if (model.LogoFile != null && model.LogoFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(settings.LogoUrl) && settings.LogoUrl != "/images/default-logo.png")
            {
                var oldPath = Path.Combine(_environment.WebRootPath, settings.LogoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logo");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.LogoFile.CopyToAsync(stream);
            }
            settings.LogoUrl = $"/uploads/logo/{uniqueFileName}";
        }

        await _siteSettingsService.SaveSettingsAsync(settings);
        TempData["SuccessMessage"] = "Настройки успешно сохранены";
        return RedirectToAction(nameof(General));
    }

    [HttpGet("Contracts")]
    public IActionResult Contracts()
    {
        var templatePath = _configuration["ContractTemplatePath"];
        var model = new ContractTemplateViewModel
        {
            CurrentTemplatePath = templatePath
        };
        return View(model);
    }

    [HttpPost("Contracts")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadTemplate(ContractTemplateViewModel model)
    {
        if (model.TemplateFile == null || model.TemplateFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Файл не выбран";
            return RedirectToAction(nameof(Contracts));
        }

        var ext = Path.GetExtension(model.TemplateFile.FileName).ToLower();
        if (ext != ".docx")
        {
            TempData["ErrorMessage"] = "Допустим только формат .docx";
            return RedirectToAction(nameof(Contracts));
        }

        var filePath = Path.Combine(_environment.WebRootPath, "templates", "contract_template.docx");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.TemplateFile.CopyToAsync(stream);
        }

        _configuration["ContractTemplatePath"] = filePath;

        TempData["SuccessMessage"] = "Шаблон успешно загружен";
        return RedirectToAction(nameof(Contracts));
    }

    [HttpGet("RenterRequirements")]
    public IActionResult RenterRequirements()
    {
        var settings = _siteSettingsService.GetSettings();
        var model = new RenterRequirementsViewModel
        {
            Requirements = settings.RenterRequirements ?? new List<RenterRequirementDto>()
        };
        return View(model);
    }

    [HttpPost("RenterRequirements")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenterRequirements(RenterRequirementsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var settings = _siteSettingsService.GetSettings();
        // Очищаем null-элементы (если пользователь удалил строки)
        settings.RenterRequirements = model.Requirements
            .Where(r => !string.IsNullOrWhiteSpace(r.Question) || !string.IsNullOrWhiteSpace(r.ExpectedAnswer))
            .ToList();

        await _siteSettingsService.SaveSettingsAsync(settings);
        TempData["SuccessMessage"] = "Требования к арендатору сохранены";
        return RedirectToAction(nameof(RenterRequirements));
    }
}