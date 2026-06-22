using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;

namespace CarRental.DAL.Repositories;

public class PageRepository : Repository<Page>, IPageRepository
{
    public PageRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Page?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
    }

    public async Task<IEnumerable<Page>> GetActivePagesAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Page>> GetPagesWithPagingAsync(int page, int pageSize)
    {
        return await _dbSet
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetActivePagesCountAsync()
    {
        return await _dbSet.CountAsync(p => p.IsActive);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}