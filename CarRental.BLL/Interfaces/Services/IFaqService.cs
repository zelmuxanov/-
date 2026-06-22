using CarRental.BLL.DTOs.Faq;

namespace CarRental.BLL.Interfaces.Services;

public interface IFaqService
{
    Task<FaqDto> GetFaqByIdAsync(Guid id);
    Task<IEnumerable<FaqDto>> GetAllFaqsAsync();
    Task<IEnumerable<FaqDto>> GetActiveFaqsAsync();
    Task<IEnumerable<FaqDto>> GetFaqsByCategoryAsync(string category);
    Task<IEnumerable<FaqDto>> GetFaqsWithPagingAsync(int page, int pageSize);
    Task<int> GetTotalFaqsCountAsync();
    Task<int> GetActiveFaqsCountAsync();
    Task<FaqDto> CreateFaqAsync(FaqCreateDto faqDto);
    Task<FaqDto> UpdateFaqAsync(FaqUpdateDto faqDto);
    Task<bool> DeleteFaqAsync(Guid id);
    Task<bool> ToggleFaqStatusAsync(Guid id);
    Task<bool> ReorderFaqsAsync(Dictionary<Guid, int> faqOrders);
}