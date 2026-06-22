using CarRental.BLL.DTOs.Document;

namespace CarRental.Web.ViewModels;

public class DocumentsIndexViewModel
{
    public DocumentDto? Passport { get; set; }
    public DocumentDto? DriverLicense { get; set; }
    public List<DocumentDto> OtherDocuments { get; set; } = new();
}