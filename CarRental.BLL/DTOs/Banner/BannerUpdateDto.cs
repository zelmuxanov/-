using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Banner;

public class BannerUpdateDto
{
    [Required(ErrorMessage = "ID обязателен")]
    public Guid Id { get; set; }
    
    [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
    public string? Title { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Ссылка на изображение обязательна")]
    [StringLength(500, ErrorMessage = "Ссылка на изображение не должна превышать 500 символов")]
    [Url(ErrorMessage = "Неверный формат URL")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Ссылка не должна превышать 200 символов")]
    [Url(ErrorMessage = "Неверный формат URL")]
    public string? Link { get; set; }
    
    [Range(0, 100, ErrorMessage = "Порядок отображения должен быть от 0 до 100")]
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [StringLength(50, ErrorMessage = "Текст кнопки не должен превышать 50 символов")]
    public string? ButtonText { get; set; }
    
    [Required(ErrorMessage = "Тип баннера обязателен")]
    public BannerType BannerType { get; set; }
    public BannerMediaType MediaType { get; set; }
    public string? VideoUrl { get; set; }
    public bool VideoAutoplay { get; set; }
    public bool VideoMuted { get; set; }
    public bool VideoLoop { get; set; }
    public bool VideoControls { get; set; }
    public string ObjectFit { get; set; } = "cover";
    public string ObjectPosition { get; set; } = "center";
}