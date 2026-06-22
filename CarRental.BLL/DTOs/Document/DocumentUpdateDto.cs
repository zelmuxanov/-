namespace CarRental.BLL.DTOs.Document;

public class DocumentUpdateDto
{
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuedBy { get; set; }
    public string? BirthDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? RegistrationAddress { get; set; }
}