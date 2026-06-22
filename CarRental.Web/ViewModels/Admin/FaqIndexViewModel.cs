using CarRental.BLL.DTOs.Faq;

namespace CarRental.Web.ViewModels.Admin;

public class FaqIndexViewModel
{
    public List<FaqDto> Faqs { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string? CategoryFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public string? Search { get; set; }
    public List<string> Categories { get; set; } = new();
}