using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Document>> GetPendingAsync();
    Task<Document?> GetByIdWithUserAsync(Guid id);
    Task<IEnumerable<Document>> GetByUserIdWithUserAsync(Guid userId);
    Task<IEnumerable<Document>> GetPendingWithUserAsync();
}   