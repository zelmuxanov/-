using CarRental.BLL.DTOs.Banner;
using CarRental.Domain.Enums;

namespace CarRental.BLL.Interfaces.Services;

public interface IBannerService
{
    Task<BannerDto> GetBannerByIdAsync(Guid id);
    Task<IEnumerable<BannerDto>> GetAllBannersAsync();
    Task<IEnumerable<BannerDto>> GetActiveBannersAsync();
    Task<IEnumerable<BannerDto>> GetBannersByTypeAsync(BannerType bannerType);
    Task<IEnumerable<BannerDto>> GetBannersWithPagingAsync(int page, int pageSize);
    Task<int> GetTotalBannersCountAsync();
    Task<int> GetActiveBannersCountAsync();
    Task<BannerDto> CreateBannerAsync(BannerCreateDto bannerDto);
    Task<BannerDto> UpdateBannerAsync(BannerUpdateDto bannerDto);
    Task<bool> DeleteBannerAsync(Guid id);
    Task<bool> ToggleBannerStatusAsync(Guid id);
    Task<bool> ReorderBannersAsync(Dictionary<Guid, int> bannerOrders);
}