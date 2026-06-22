using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.User;
using CarRental.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Spire.Doc;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CarRental.Web.Services;

public class ContractService : IContractService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IUserService _userService;
    private readonly IDocumentService _documentService;
    private readonly ILogger<ContractService> _logger;

    public ContractService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IUserService userService,
        IDocumentService documentService,
        ILogger<ContractService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _userService = userService;
        _documentService = documentService;
        _logger = logger;
    }

    public async Task<string> GenerateContractDocxAsync(Booking booking)
    {
        var templatePath = _configuration["ContractTemplatePath"];
        if (string.IsNullOrEmpty(templatePath))
            templatePath = Path.Combine(_environment.WebRootPath, "templates", "contract_template.docx");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Шаблон договора не найден", templatePath);

        var document = new Spire.Doc.Document();
        document.LoadFromFile(templatePath);

        // Получаем данные, если есть пользователь
        UserDto? user = null;
        IEnumerable<BLL.DTOs.Document.DocumentDto> userDocs = Enumerable.Empty<BLL.DTOs.Document.DocumentDto>();

        if (booking.UserId.HasValue)
        {
            user = await _userService.GetUserByIdAsync(booking.UserId.Value);
            userDocs = await _documentService.GetUserDocumentsAsync(booking.UserId.Value);
        }

        var passport = userDocs.FirstOrDefault(d => d.DocumentType == Domain.Enums.DocumentType.Passport);
        var driverLicense = userDocs.FirstOrDefault(d => d.DocumentType == Domain.Enums.DocumentType.DriverLicense);

        var replacements = new Dictionary<string, string>
        {
            ["[номер]"] = booking.ContractNumber ?? booking.Id.ToString().Substring(0, 8),
            ["[дата]"] = DateTime.Now.ToString("dd.MM.yyyy"),
            ["[фио]"] = $"{user?.LastName} {user?.FirstName} {user?.MiddleName}".Trim(),
            ["[фио_инициалы]"] = GetInitials(user),
            ["[дата_рождения]"] = user?.BirthDate.ToString("dd.MM.yyyy") ?? "—",
            ["[паспорт_серия]"] = GetPassportSeries(passport?.DocumentNumber),
            ["[паспорт_номер]"] = GetPassportNumber(passport?.DocumentNumber),
            ["[паспорт_выдан]"] = passport?.IssuedBy ?? "—",
            ["[паспорт_адрес]"] = passport?.RegistrationAddress ?? "—",
            ["[ву_серия]"] = GetLicenseSeries(driverLicense?.DocumentNumber),
            ["[ву_номер]"] = GetLicenseNumber(driverLicense?.DocumentNumber),
            ["[ву_дата_выдачи]"] = driverLicense?.IssueDate?.ToString("dd.MM.yyyy") ?? "—",
            ["[ву_стаж]"] = driverLicense?.IssueDate != null ? $"с {driverLicense.IssueDate.Value:dd.MM.yyyy}" : "—",
            ["[марка]"] = booking.Car.Brand,
            ["[модель]"] = booking.Car.Model,
            ["[год_выпуска]"] = booking.Car.Year.ToString(),
            ["[цвет]"] = booking.Car.Color ?? "—",
            ["[гос_номер]"] = booking.Car.LicensePlate ?? "—",
            ["[вин]"] = booking.Car.VIN ?? "—",
            ["[телефон]"] = user?.PhoneNumber ?? "—",
            ["[email]"] = user?.Email ?? "—",
            ["[дата_начало]"] = booking.StartDate.ToString("dd MMMM yyyy", new CultureInfo("ru-RU")),
            ["[дата_конец]"] = booking.EndDate.ToString("dd MMMM yyyy", new CultureInfo("ru-RU")),
            ["[аренда_суток]"] = (booking.EndDate - booking.StartDate).Days.ToString(),
            ["[аренда_стоимость]"] = booking.TotalPrice.ToString("N0") + " ₽"
        };

        foreach (var rep in replacements)
        {
            document.Replace(rep.Key, rep.Value, true, true);
        }

        using var stream = new MemoryStream();
        document.SaveToStream(stream, FileFormat.Docx);
        var docxBytes = stream.ToArray();

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "contracts");
        Directory.CreateDirectory(uploadsFolder);
        var safeFileName = $"{Guid.NewGuid()}_contract_{booking.Id}.docx";
        var filePath = Path.Combine(uploadsFolder, safeFileName);
        await File.WriteAllBytesAsync(filePath, docxBytes);
        var docxUrl = $"/uploads/contracts/{safeFileName}";

        _logger.LogInformation("Договор сохранён: {DocxUrl}", docxUrl);
        return docxUrl;
    }

    public async Task<string> GenerateContractPdfAsync(Booking booking) =>
        await GenerateContractDocxAsync(booking);

    private static string GetInitials(UserDto? user)
    {
        if (user == null) return "—";
        var lastName = user.LastName?.Trim() ?? "";
        var firstName = user.FirstName?.Trim() ?? "";
        var middleName = user.MiddleName?.Trim() ?? "";
        var result = lastName;
        if (!string.IsNullOrEmpty(firstName)) result += $" {firstName[0]}.";
        if (!string.IsNullOrEmpty(middleName)) result += $" {middleName[0]}.";
        return result;
    }

    private static string GetPassportSeries(string? docNumber)
    {
        if (string.IsNullOrEmpty(docNumber)) return "";
        var cleaned = Regex.Replace(docNumber, @"\s|-", "");
        return cleaned.Length >= 4 ? cleaned[..4] : cleaned;
    }
    private static string GetPassportNumber(string? docNumber)
    {
        if (string.IsNullOrEmpty(docNumber)) return "";
        var cleaned = Regex.Replace(docNumber, @"\s|-", "");
        return cleaned.Length > 4 ? cleaned[4..] : "";
    }
    private static string GetLicenseSeries(string? docNumber) => GetPassportSeries(docNumber);
    private static string GetLicenseNumber(string? docNumber) => GetPassportNumber(docNumber);
}