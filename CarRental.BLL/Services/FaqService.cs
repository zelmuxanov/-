using AutoMapper;
using Microsoft.Extensions.Logging;
using CarRental.BLL.DTOs.Faq;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;

namespace CarRental.BLL.Services;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FaqService> _logger;

    public FaqService(IFaqRepository faqRepository, IMapper mapper, ILogger<FaqService> logger)
    {
        _faqRepository = faqRepository ?? throw new ArgumentNullException(nameof(faqRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FaqDto> GetFaqByIdAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        return _mapper.Map<FaqDto>(faq);
    }

    public async Task<IEnumerable<FaqDto>> GetAllFaqsAsync()
    {
        var faqs = await _faqRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<IEnumerable<FaqDto>> GetActiveFaqsAsync()
    {
        var faqs = await _faqRepository.GetActiveFaqsAsync();
        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<IEnumerable<FaqDto>> GetFaqsByCategoryAsync(string category)
    {
        var faqs = await _faqRepository.GetFaqsByCategoryAsync(category);
        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<IEnumerable<FaqDto>> GetFaqsWithPagingAsync(int page, int pageSize)
    {
        var faqs = await _faqRepository.GetFaqsWithPagingAsync(page, pageSize);
        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<int> GetTotalFaqsCountAsync()
    {
        return await _faqRepository.CountAsync();
    }

    public async Task<int> GetActiveFaqsCountAsync()
    {
        return await _faqRepository.GetActiveFaqsCountAsync();
    }

    public async Task<FaqDto> CreateFaqAsync(FaqCreateDto faqDto)
    {
        var faq = _mapper.Map<FaqItem>(faqDto);
        await _faqRepository.AddAsync(faq);
        await _faqRepository.SaveChangesAsync();
        return _mapper.Map<FaqDto>(faq);
    }

    public async Task<FaqDto> UpdateFaqAsync(FaqUpdateDto faqDto)
    {
        var existing = await _faqRepository.GetByIdAsync(faqDto.Id);
        if (existing == null)
            throw new KeyNotFoundException($"FAQ с ID {faqDto.Id} не найден");

        _mapper.Map(faqDto, existing);
        existing.UpdatedAt = DateTime.UtcNow;
        _faqRepository.Update(existing);
        await _faqRepository.SaveChangesAsync();
        return _mapper.Map<FaqDto>(existing);
    }

    public async Task<bool> DeleteFaqAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq == null) return false;
        _faqRepository.Delete(faq);
        await _faqRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFaqStatusAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq == null) return false;
        faq.IsActive = !faq.IsActive;
        faq.UpdatedAt = DateTime.UtcNow;
        _faqRepository.Update(faq);
        await _faqRepository.SaveChangesAsync();
        return faq.IsActive;
    }

    public async Task<bool> ReorderFaqsAsync(Dictionary<Guid, int> faqOrders)
    {
        foreach (var order in faqOrders)
        {
            var faq = await _faqRepository.GetByIdAsync(order.Key);
            if (faq != null)
            {
                faq.DisplayOrder = order.Value;
                faq.UpdatedAt = DateTime.UtcNow;
                _faqRepository.Update(faq);
            }
        }
        await _faqRepository.SaveChangesAsync();
        return true;
    }
}