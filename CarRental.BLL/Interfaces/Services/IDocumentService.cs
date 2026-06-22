using CarRental.BLL.DTOs.Document;
using CarRental.Domain.Entities;

namespace CarRental.BLL.Interfaces.Services;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(DocumentUploadDto uploadDto);
    Task<DocumentDto?> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId);
    Task<bool> UpdateDocumentStatusAsync(Guid documentId, string status, string? adminComment = null);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId);
    Task<IEnumerable<DocumentDto>> GetPendingDocumentsAsync();
    Task<bool> UpdateDocumentDetailsAsync(Guid documentId, DocumentUpdateDto updateDto);
    Task<int> GetDrivingExperienceAsync(Guid userId); 
}