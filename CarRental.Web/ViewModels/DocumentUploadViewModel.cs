using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CarRental.Web.ViewModels;

public class DocumentUploadViewModel
{
    [Required(ErrorMessage = "Выберите тип документа")]
    public DocumentType DocumentType { get; set; }

    [Required(ErrorMessage = "Выберите основной файл")]
    public IFormFile File { get; set; } = null!;

    public IFormFile? File2 { get; set; }

    public string? Description { get; set; }

    // Общие поля
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? IssuedBy { get; set; }

    // Паспорт
    public string? BirthDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? RegistrationAddress { get; set; }

    // В/У
    public DateTime? ExpiryDate { get; set; }
}