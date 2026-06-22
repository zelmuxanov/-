using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IPageRepository : IRepository<Page>
{
    Task<Page?> GetBySlugAsync(string slug);
    Task<IEnumerable<Page>> GetActivePagesAsync();
    Task<IEnumerable<Page>> GetPagesWithPagingAsync(int page, int pageSize);
    Task<int> GetActivePagesCountAsync();
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
}