using CarRental.BLL.DTOs.Page;

namespace CarRental.BLL.Interfaces.Services;

public interface IPageService
{
    Task<PageDto?> GetPageByIdAsync(Guid id);
    Task<PageDto?> GetPageBySlugAsync(string slug);
    Task<IEnumerable<PageDto>> GetAllPagesAsync();
    Task<IEnumerable<PageDto>> GetActivePagesAsync();
    Task<IEnumerable<PageDto>> GetPagesWithPagingAsync(int page, int pageSize);
    Task<int> GetTotalPagesCountAsync();
    Task<int> GetActivePagesCountAsync();
    Task<PageDto> CreatePageAsync(PageCreateDto pageDto);
    Task<PageDto> UpdatePageAsync(PageUpdateDto pageDto);
    Task<bool> DeletePageAsync(Guid id);
    Task<bool> TogglePageStatusAsync(Guid id);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null);
}