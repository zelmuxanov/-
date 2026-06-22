using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Document : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Первый файл (лицевая сторона)
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    
    // Второй файл (оборотная сторона / прописка)
    public string? FileName2 { get; set; }
    public string? FilePath2 { get; set; }
    
    public DocumentType DocumentType { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Description { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    // Реквизиты
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuedBy { get; set; }
    public string? BirthDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? RegistrationAddress { get; set; }
    
    public virtual User User { get; set; } = null!;
}