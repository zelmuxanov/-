using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Banner : BaseEntity
{
    [MaxLength(200)]
    public string? Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Link { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [MaxLength(50)]
    public string? ButtonText { get; set; } = "Подробнее";
    
    // Устанавливаем явное значение по умолчанию
    public BannerType BannerType { get; set; } = BannerType.MainCarousel; // <-- ЯВНОЕ значение
    
    // ИЛИ альтернатива: сделать nullable (если допустимо в бизнес-логике)
    // public BannerType? BannerType { get; set; } = BannerType.MainCarousel;
    public Banner()
    {
        CreatedAt = DateTime.UtcNow;
    }
    public BannerMediaType MediaType { get; set; } = BannerMediaType.Image;
    [MaxLength(500)]
    public string? VideoUrl { get; set; }
    public bool VideoAutoplay { get; set; } = true;
    public bool VideoMuted { get; set; } = true;
    public bool VideoLoop { get; set; } = false;
    public bool VideoControls { get; set; } = false;
    public string ObjectFit { get; set; } = "cover";
    public string ObjectPosition { get; set; } = "center";
}