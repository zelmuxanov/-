using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CarRental.BLL.DTOs.Document;

public class DocumentUploadDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
    
    public IFormFile? File2 { get; set; } // второй файл

    public string? Description { get; set; }

    // Реквизиты
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuedBy { get; set; }
    public string? BirthDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? RegistrationAddress { get; set; }
}