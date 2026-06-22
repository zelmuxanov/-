using CarRental.BLL.DTOs.Document;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Web.Controllers;

[Authorize]
[Route("file")]
public class FileController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorageService;
    private readonly UserManager<User> _userManager;
    private readonly IBookingService _bookingService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileController> _logger;

    public FileController(
        IDocumentService documentService,
        IFileStorageService fileStorageService,
        UserManager<User> userManager,
        IBookingService bookingService,
        IWebHostEnvironment environment,
        ILogger<FileController> logger)
    {
        _documentService = documentService;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
        _bookingService = bookingService;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("document/{id}")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin");
        var isOwner = currentUser?.Id == document.UserId;

        if (!isAdmin && !isOwner) return Forbid();

        try
        {
            var fileBytes = await _fileStorageService.ReadFileAsync(document.FilePath);
            var contentType = GetContentType(Path.GetExtension(document.FilePath));
            var contentDisposition = contentType.StartsWith("image/") ? "inline" : "attachment";
            Response.Headers.Append("Content-Disposition", $"{contentDisposition}; filename=\"{document.FileName}\"");
            return File(fileBytes, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при чтении файла документа {DocumentId}", id);
            return StatusCode(500, "Ошибка при получении файла");
        }
    }

    [HttpGet("document/{id}/second")]
    public async Task<IActionResult> GetDocumentSecond(Guid id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null) return NotFound();
        if (string.IsNullOrEmpty(document.FilePath2)) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin");
        var isOwner = currentUser?.Id == document.UserId;
        if (!isAdmin && !isOwner) return Forbid();

        try
        {
            var fileBytes = await _fileStorageService.ReadFileAsync(document.FilePath2);
            var contentType = GetContentType(Path.GetExtension(document.FilePath2));
            var contentDisposition = contentType.StartsWith("image/") ? "inline" : "attachment";
            Response.Headers.Append("Content-Disposition", $"{contentDisposition}; filename=\"{document.FileName2}\"");
            return File(fileBytes, contentType);
        }
        catch
        {
            return NotFound();
        }
    }

    private string GetContentType(string extension) => extension.ToLower() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
    [HttpGet("contract/{bookingId}")]
    [Authorize]
    public async Task<IActionResult> DownloadContract(Guid bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking == null) return NotFound();

        // Проверка прав
        var currentUser = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin");
        var isOwner = currentUser?.Id == booking.UserId;
        if (!isAdmin && !isOwner) return Forbid();

        if (string.IsNullOrEmpty(booking.ContractUrl))
            return NotFound("Договор не найден в базе");

        // Полный путь к файлу
        var filePath = Path.Combine(_environment.WebRootPath, booking.ContractUrl.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
            return NotFound($"Файл не найден на диске: {filePath}");

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
        {
            FileDownloadName = $"Договор_{booking.ContractNumber}.docx",
            EnableRangeProcessing = true
        };
    }
}