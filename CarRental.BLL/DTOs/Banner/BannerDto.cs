using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Banner;

public class BannerDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Link { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ButtonText { get; set; }
    public BannerType BannerType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public BannerMediaType MediaType { get; set; }
    public string? VideoUrl { get; set; }
    public bool VideoAutoplay { get; set; }
    public bool VideoMuted { get; set; }
    public bool VideoLoop { get; set; }
    public bool VideoControls { get; set; }
    public string ObjectFit { get; set; } = "cover";
    public string ObjectPosition { get; set; } = "center";
}