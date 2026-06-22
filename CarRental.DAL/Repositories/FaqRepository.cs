using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;

namespace CarRental.DAL.Repositories;

public class FaqRepository : Repository<FaqItem>, IFaqRepository
{
    public FaqRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<FaqItem>> GetActiveFaqsAsync()
    {
        return await _dbSet
            .Where(f => f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FaqItem>> GetFaqsByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(f => f.Category == category && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<FaqItem>> GetFaqsWithPagingAsync(int page, int pageSize)
    {
        return await _dbSet
            .OrderBy(f => f.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetActiveFaqsCountAsync()
    {
        return await _dbSet.CountAsync(f => f.IsActive);
    }
}