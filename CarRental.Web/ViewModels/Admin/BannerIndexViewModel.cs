using CarRental.BLL.DTOs.Banner;

namespace CarRental.Web.ViewModels.Admin;

public class BannerIndexViewModel
{
    public IEnumerable<BannerDto> Banners { get; set; } = new List<BannerDto>();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public CarRental.Domain.Enums.BannerType? TypeFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public string? Search { get; set; }
}