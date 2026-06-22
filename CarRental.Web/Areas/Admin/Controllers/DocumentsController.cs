using CarRental.BLL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.DTOs.Document;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DocumentsController : BaseAdminController
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    private Guid? GetFilterUserId()
    {
        if (TempData.TryGetValue("FilterUserId", out var userIdStr) && Guid.TryParse(userIdStr?.ToString(), out var userId))
            return userId;
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? userId)
    {
        if (userId.HasValue)
        {
            TempData["LastUserId"] = userId.Value;
        }
        else
        {
            TempData.Remove("LastUserId");
        }

        IEnumerable<DocumentDto> documents;
        if (userId.HasValue)
        {
            documents = await _documentService.GetUserDocumentsAsync(userId.Value);
            ViewBag.FilterUserId = userId;
        }
        else
        {
            documents = await _documentService.GetPendingDocumentsAsync();
            ViewBag.FilterUserId = null;
        }
        return View(documents);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null) return NotFound();
        ViewBag.ReturnUserId = GetFilterUserId();
        return View(document);
    }

    [HttpPost("verify/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(Guid id, string? comment)
    {
        await _documentService.UpdateDocumentStatusAsync(id, "Verified", comment);
        TempData["SuccessMessage"] = "Документ подтверждён.";
        var userId = GetFilterUserId();
        return RedirectToAction(nameof(Index), new { area = "Admin", userId = userId });
    }

    [HttpPost("reject/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string rejectReason)
    {
        if (string.IsNullOrWhiteSpace(rejectReason))
        {
            TempData["ErrorMessage"] = "Укажите причину отклонения.";
            return RedirectToAction(nameof(Details), new { id });
        }
        await _documentService.UpdateDocumentStatusAsync(id, "Rejected", rejectReason);
        TempData["SuccessMessage"] = "Документ отклонён.";
        var userId = GetFilterUserId();
        return RedirectToAction(nameof(Index), new { area = "Admin", userId = userId });
    }

    [HttpPost("update/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, DocumentUpdateDto model)
    {
        var result = await _documentService.UpdateDocumentDetailsAsync(id, model);
        if (result)
            TempData["SuccessMessage"] = "Данные документа обновлены.";
        else
            TempData["ErrorMessage"] = "Не удалось обновить данные.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null)
        {
            TempData["ErrorMessage"] = "Документ не найден.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _documentService.DeleteDocumentAsync(id, document.UserId);
        if (result)
        {
            TempData["SuccessMessage"] = $"Документ {document.DocumentNumber} успешно удалён.";
            _logger.LogInformation("Администратор удалил документ {DocumentId} пользователя {UserId}", id, document.UserId);
        }
        else
        {
            TempData["ErrorMessage"] = "Не удалось удалить документ.";
        }
        var userId = GetFilterUserId();
        return RedirectToAction(nameof(Index), new { area = "Admin", userId = userId });
    }
}