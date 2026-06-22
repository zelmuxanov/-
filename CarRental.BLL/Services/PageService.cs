using AutoMapper;
using Microsoft.Extensions.Logging;
using CarRental.BLL.DTOs.Page;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;

namespace CarRental.BLL.Services;

public class PageService : IPageService
{
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PageService> _logger;

    public PageService(IPageRepository pageRepository, IMapper mapper, ILogger<PageService> logger)
    {
        _pageRepository = pageRepository ?? throw new ArgumentNullException(nameof(pageRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PageDto?> GetPageByIdAsync(Guid id)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        return _mapper.Map<PageDto>(page);
    }

    public async Task<PageDto?> GetPageBySlugAsync(string slug)
    {
        var page = await _pageRepository.GetBySlugAsync(slug);
        return _mapper.Map<PageDto>(page);
    }

    public async Task<IEnumerable<PageDto>> GetAllPagesAsync()
    {
        var pages = await _pageRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PageDto>>(pages);
    }

    public async Task<IEnumerable<PageDto>> GetActivePagesAsync()
    {
        var pages = await _pageRepository.GetActivePagesAsync();
        return _mapper.Map<IEnumerable<PageDto>>(pages);
    }

    public async Task<IEnumerable<PageDto>> GetPagesWithPagingAsync(int page, int pageSize)
    {
        var pages = await _pageRepository.GetPagesWithPagingAsync(page, pageSize);
        return _mapper.Map<IEnumerable<PageDto>>(pages);
    }

    public async Task<int> GetTotalPagesCountAsync()
    {
        return await _pageRepository.CountAsync();
    }

    public async Task<int> GetActivePagesCountAsync()
    {
        return await _pageRepository.GetActivePagesCountAsync();
    }

    public async Task<PageDto> CreatePageAsync(PageCreateDto pageDto)
    {
        var page = _mapper.Map<Page>(pageDto);
        await _pageRepository.AddAsync(page);
        await _pageRepository.SaveChangesAsync();
        return _mapper.Map<PageDto>(page);
    }

    public async Task<PageDto> UpdatePageAsync(PageUpdateDto pageDto)
    {
        var existing = await _pageRepository.GetByIdAsync(pageDto.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Страница с ID {pageDto.Id} не найдена");

        _mapper.Map(pageDto, existing);
        existing.UpdatedAt = DateTime.UtcNow;
        _pageRepository.Update(existing);
        await _pageRepository.SaveChangesAsync();
        return _mapper.Map<PageDto>(existing);
    }

    public async Task<bool> DeletePageAsync(Guid id)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        if (page == null) return false;
        _pageRepository.Delete(page);
        await _pageRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TogglePageStatusAsync(Guid id)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        if (page == null) return false;
        page.IsActive = !page.IsActive;
        page.UpdatedAt = DateTime.UtcNow;
        _pageRepository.Update(page);
        await _pageRepository.SaveChangesAsync();
        return page.IsActive;
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null)
    {
        return !await _pageRepository.SlugExistsAsync(slug, excludeId);
    }
}