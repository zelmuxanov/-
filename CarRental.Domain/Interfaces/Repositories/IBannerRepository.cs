using CarRental.Domain.Enums;
using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IBannerRepository : IRepository<Banner>
{
    Task<IEnumerable<Banner>> GetActiveBannersAsync();
    Task<IEnumerable<Banner>> GetBannersByTypeAsync(BannerType bannerType);
    Task<IEnumerable<Banner>> GetBannersWithPagingAsync(int page, int pageSize);
    Task<int> GetActiveBannersCountAsync();
}