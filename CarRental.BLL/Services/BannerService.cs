using AutoMapper;
using Microsoft.Extensions.Logging;
using CarRental.BLL.DTOs.Banner;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.Domain.Enums;

namespace CarRental.BLL.Services;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _bannerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BannerService> _logger;

    public BannerService(
        IBannerRepository bannerRepository,
        IMapper mapper,
        ILogger<BannerService> logger)
    {
        _bannerRepository = bannerRepository ?? throw new ArgumentNullException(nameof(bannerRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BannerDto> GetBannerByIdAsync(Guid id)
    {
        try
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            return _mapper.Map<BannerDto>(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении баннера с ID: {BannerId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<BannerDto>> GetAllBannersAsync()
    {
        try
        {
            var banners = await _bannerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех баннеров");
            throw;
        }
    }

    public async Task<IEnumerable<BannerDto>> GetActiveBannersAsync()
    {
        try
        {
            var banners = await _bannerRepository.GetActiveBannersAsync();
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных баннеров");
            throw;
        }
    }

    public async Task<IEnumerable<BannerDto>> GetBannersByTypeAsync(BannerType bannerType)
    {
        try
        {
            var banners = await _bannerRepository.GetBannersByTypeAsync(bannerType);
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении баннеров по типу: {BannerType}", bannerType);
            throw;
        }
    }

    public async Task<IEnumerable<BannerDto>> GetBannersWithPagingAsync(int page, int pageSize)
    {
        try
        {
            var banners = await _bannerRepository.GetBannersWithPagingAsync(page, pageSize);
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении баннеров с пагинацией: Страница {Page}, Размер {PageSize}", page, pageSize);
            throw;
        }
    }

    public async Task<int> GetTotalBannersCountAsync()
    {
        try
        {
            return await _bannerRepository.CountAsync(); // <-- Теперь это работает
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении общего количества баннеров");
            throw;
        }
    }

    public async Task<int> GetActiveBannersCountAsync()
    {
        try
        {
            return await _bannerRepository.GetActiveBannersCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении количества активных баннеров");
            throw;
        }
    }

    public async Task<BannerDto> CreateBannerAsync(BannerCreateDto bannerDto)
    {
        try
        {
            // Преобразуем DateTime в UTC перед сохранением
            if (bannerDto.StartDate.HasValue && bannerDto.StartDate.Value.Kind != DateTimeKind.Utc)
            {
                bannerDto.StartDate = DateTime.SpecifyKind(bannerDto.StartDate.Value, DateTimeKind.Utc);
            }
            
            if (bannerDto.EndDate.HasValue && bannerDto.EndDate.Value.Kind != DateTimeKind.Utc)
            {
                bannerDto.EndDate = DateTime.SpecifyKind(bannerDto.EndDate.Value, DateTimeKind.Utc);
            }
            
            var banner = _mapper.Map<Banner>(bannerDto);
            
            await _bannerRepository.AddAsync(banner);
            await _bannerRepository.SaveChangesAsync();
            
            return _mapper.Map<BannerDto>(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании баннера");
            throw;
        }
    }

    public async Task<BannerDto> UpdateBannerAsync(BannerUpdateDto bannerDto)
    {
        try
        {
            var existingBanner = await _bannerRepository.GetByIdAsync(bannerDto.Id);
            if (existingBanner == null)
                throw new KeyNotFoundException($"Баннер с ID {bannerDto.Id} не найден");

            // Преобразуем DateTime в UTC перед сохранением
            if (bannerDto.StartDate.HasValue && bannerDto.StartDate.Value.Kind != DateTimeKind.Utc)
            {
                bannerDto.StartDate = DateTime.SpecifyKind(bannerDto.StartDate.Value, DateTimeKind.Utc);
            }
            
            if (bannerDto.EndDate.HasValue && bannerDto.EndDate.Value.Kind != DateTimeKind.Utc)
            {
                bannerDto.EndDate = DateTime.SpecifyKind(bannerDto.EndDate.Value, DateTimeKind.Utc);
            }

            _mapper.Map(bannerDto, existingBanner);
            existingBanner.UpdatedAt = DateTime.UtcNow;
            
            _bannerRepository.Update(existingBanner);
            await _bannerRepository.SaveChangesAsync();
            
            return _mapper.Map<BannerDto>(existingBanner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении баннера с ID: {BannerId}", bannerDto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteBannerAsync(Guid id)
    {
        try
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                return false;

            _bannerRepository.Delete(banner);
            await _bannerRepository.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении баннера с ID: {BannerId}", id);
            throw;
        }
    }

    public async Task<bool> ToggleBannerStatusAsync(Guid id)
    {
        try
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                return false;

            banner.IsActive = !banner.IsActive;
            banner.UpdatedAt = DateTime.UtcNow;
            
            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();
            
            return banner.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переключении статуса баннера с ID: {BannerId}", id);
            throw;
        }
    }

    public async Task<bool> ReorderBannersAsync(Dictionary<Guid, int> bannerOrders)
    {
        try
        {
            foreach (var order in bannerOrders)
            {
                var banner = await _bannerRepository.GetByIdAsync(order.Key);
                if (banner != null)
                {
                    banner.DisplayOrder = order.Value;
                    banner.UpdatedAt = DateTime.UtcNow;
                    _bannerRepository.Update(banner);
                }
            }
            
            await _bannerRepository.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении порядка баннеров");
            throw;
        }
    }
}