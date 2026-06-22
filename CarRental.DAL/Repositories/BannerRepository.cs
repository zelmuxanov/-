using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class BannerRepository : Repository<Banner>, IBannerRepository
{
    public BannerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .Where(b => b.IsActive &&
                   (!b.StartDate.HasValue || b.StartDate <= now) &&
                   (!b.EndDate.HasValue || b.EndDate >= now))
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Banner>> GetBannersByTypeAsync(BannerType bannerType)
    {
        return await _dbSet
            .Where(b => b.BannerType == bannerType && b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Banner>> GetBannersWithPagingAsync(int page, int pageSize)
    {
        return await _dbSet
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetActiveBannersCountAsync()
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .CountAsync(b => b.IsActive &&
                   (!b.StartDate.HasValue || b.StartDate <= now) &&
                   (!b.EndDate.HasValue || b.EndDate >= now));
    }
}