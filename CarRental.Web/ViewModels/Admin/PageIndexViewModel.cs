using CarRental.BLL.DTOs.Page;

namespace CarRental.Web.ViewModels.Admin;

public class PageIndexViewModel
{
    public List<PageDto> Pages { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool? IsActiveFilter { get; set; }
    public string? Search { get; set; }
}