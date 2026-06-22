using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IFaqRepository : IRepository<FaqItem>
{
    Task<IEnumerable<FaqItem>> GetActiveFaqsAsync();
    Task<IEnumerable<FaqItem>> GetFaqsByCategoryAsync(string category);
    Task<IEnumerable<FaqItem>> GetFaqsWithPagingAsync(int page, int pageSize);
    Task<int> GetActiveFaqsCountAsync();
}