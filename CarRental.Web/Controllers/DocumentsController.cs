using CarRental.BLL.DTOs.Document;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Web.Controllers;

[Authorize]
[Route("Documents")]
public class DocumentsController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        UserManager<User> userManager,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var documents = await _documentService.GetUserDocumentsAsync(user.Id);
        
        // Группируем по типу для удобства отображения в представлении
        var model = new DocumentsIndexViewModel
        {
            Passport = documents.FirstOrDefault(d => d.DocumentType == DocumentType.Passport),
            DriverLicense = documents.FirstOrDefault(d => d.DocumentType == DocumentType.DriverLicense),
            OtherDocuments = documents.Where(d => d.DocumentType != DocumentType.Passport && d.DocumentType != DocumentType.DriverLicense).ToList()
        };

        return View(model);
    }

    [HttpGet("Upload")]
    public IActionResult Upload(DocumentType? type = null)
    {
        var model = new DocumentUploadViewModel
        {
            DocumentType = type ?? DocumentType.Passport
        };
        return View(model);
    }

    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        // Безопасное извлечение данных из формы
        var form = Request.Form;
        
        // Тип документа
        if (!form.TryGetValue("DocumentType", out var docTypeValue) || string.IsNullOrEmpty(docTypeValue))
        {
            ModelState.AddModelError("DocumentType", "Тип документа не указан.");
            return View(new DocumentUploadViewModel());
        }
        if (!Enum.TryParse<DocumentType>(docTypeValue, out var docType))
        {
            ModelState.AddModelError("DocumentType", "Некорректный тип документа.");
            return View(new DocumentUploadViewModel());
        }

        // Файлы
        var file = form.Files.GetFile("File");
        var file2 = form.Files.GetFile("File2");

        // Ручное заполнение модели
        var model = new DocumentUploadViewModel
        {
            DocumentType = docType,
            File = file!, // после проверки ниже будет гарантированно не null
            File2 = file2,
            Description = form["Description"],
            DocumentNumber = form["DocumentNumber"],
            IssuedBy = form["IssuedBy"],
            BirthDate = form["BirthDate"],
            PlaceOfBirth = form["PlaceOfBirth"],
            RegistrationAddress = form["RegistrationAddress"]
        };

        // Парсинг дат
        if (DateTime.TryParse(form["IssueDate"], out var issueDate))
            model.IssueDate = issueDate;
        if (DateTime.TryParse(form["ExpiryDate"], out var expiryDate))
            model.ExpiryDate = expiryDate;

        // Валидация
        if (model.File == null || model.File.Length == 0)
            ModelState.AddModelError("File", "Основной файл обязателен.");

        if (docType == DocumentType.Passport)
        {
            if (string.IsNullOrWhiteSpace(model.DocumentNumber))
                ModelState.AddModelError("DocumentNumber", "Укажите серию и номер паспорта");
            if (!model.IssueDate.HasValue)
                ModelState.AddModelError("IssueDate", "Укажите дату выдачи");
            if (string.IsNullOrWhiteSpace(model.BirthDate))
                ModelState.AddModelError("BirthDate", "Укажите дату рождения");
            if (string.IsNullOrWhiteSpace(model.PlaceOfBirth))
                ModelState.AddModelError("PlaceOfBirth", "Укажите место рождения");
        }
        else if (docType == DocumentType.DriverLicense)
        {
            if (string.IsNullOrWhiteSpace(model.DocumentNumber))
                ModelState.AddModelError("DocumentNumber", "Укажите серию и номер ВУ");
            if (!model.IssueDate.HasValue)
                ModelState.AddModelError("IssueDate", "Укажите дату выдачи");
            if (!model.ExpiryDate.HasValue)
                ModelState.AddModelError("ExpiryDate", "Укажите срок действия");
            else if (model.ExpiryDate <= model.IssueDate)
                ModelState.AddModelError("ExpiryDate", "Срок действия должен быть позже даты выдачи");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return View(model);
        }

        try
        {
            var dto = new DocumentUploadDto
            {
                UserId = user.Id,
                DocumentType = model.DocumentType,
                File = model.File!, // уже проверен
                File2 = model.File2,
                Description = model.Description,
                DocumentNumber = model.DocumentNumber,
                IssueDate = model.IssueDate.HasValue ? DateTime.SpecifyKind(model.IssueDate.Value, DateTimeKind.Utc) : null,
                ExpiryDate = model.ExpiryDate.HasValue ? DateTime.SpecifyKind(model.ExpiryDate.Value, DateTimeKind.Utc) : null,
                IssuedBy = model.IssuedBy,
                BirthDate = model.BirthDate,
                PlaceOfBirth = model.PlaceOfBirth,
                RegistrationAddress = model.RegistrationAddress
            };

            await _documentService.UploadDocumentAsync(dto);
            TempData["SuccessMessage"] = "Документ успешно загружен и отправлен на проверку.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке документа");
            ViewBag.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var result = await _documentService.DeleteDocumentAsync(id, user.Id);
        if (!result)
            TempData["ErrorMessage"] = "Не удалось удалить документ.";
        else
            TempData["SuccessMessage"] = "Документ удалён.";

        return RedirectToAction(nameof(Index));
    }
}