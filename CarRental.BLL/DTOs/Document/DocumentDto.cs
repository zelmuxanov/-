using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Document;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    
    public string? FileName2 { get; set; }
    public string? FilePath2 { get; set; }
    public string? FileUrl2 { get; set; }
    public DocumentType DocumentType { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuedBy { get; set; }
    public string? BirthDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? RegistrationAddress { get; set; }
}