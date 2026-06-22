using System.ComponentModel.DataAnnotations;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace CarRental.Web.ViewModels.Admin;

public class BannerCreateEditViewModel
{
    public Guid? Id { get; set; }
    
    [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
    [Display(Name = "Заголовок")]
    public string? Title { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }
    
    [Display(Name = "Изображение")]
    public IFormFile? ImageFile { get; set; }
    
    [Display(Name = "Текущее изображение")]
    public string? CurrentImageUrl { get; set; } // <-- ДОБАВЛЯЕМ ЭТО СВОЙСТВО!
    
    [StringLength(200, ErrorMessage = "Ссылка не должна превышать 200 символов")]
    [Url(ErrorMessage = "Неверный формат URL")]
    [Display(Name = "Ссылка (опционально)")]
    public string? Link { get; set; }
    
    [Range(0, 100, ErrorMessage = "Порядок отображения должен быть от 0 до 100")]
    [Display(Name = "Порядок отображения")]
    public int DisplayOrder { get; set; } = 0;
    
    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Дата начала показа")]
    [DataType(DataType.DateTime)]
    public DateTime? StartDate { get; set; }
    
    [Display(Name = "Дата окончания показа")]
    [DataType(DataType.DateTime)]
    public DateTime? EndDate { get; set; }
    
    [StringLength(50, ErrorMessage = "Текст кнопки не должен превышать 50 символов")]
    [Display(Name = "Текст кнопки")]
    public string? ButtonText { get; set; } = "Подробнее";
    
    [Required(ErrorMessage = "Тип баннера обязателен")]
    [Display(Name = "Тип баннера")]
    public BannerType BannerType { get; set; } = BannerType.MainCarousel;

    [Display(Name = "Тип медиа")]
    public BannerMediaType MediaType { get; set; } = BannerMediaType.Image;

    [Display(Name = "Ссылка на видео (для типа Видео)")]
    [Url(ErrorMessage = "Неверный формат ссылки")]
    public string? VideoUrl { get; set; }

    // Для загрузки видеофайла (если разрешим загрузку)
    public IFormFile? VideoFile { get; set; }

    // Для отображения текущего видео при редактировании
    public string? CurrentVideoUrl { get; set; }
    
    // Для dropdown списков
    public List<SelectListItem> BannerTypes { get; set; } = new List<SelectListItem>();
    [Display(Name = "Автовоспроизведение видео")]
    public bool VideoAutoplay { get; set; } = true;

    [Display(Name = "Без звука")]
    public bool VideoMuted { get; set; } = true;

    [Display(Name = "Зациклить видео")]
    public bool VideoLoop { get; set; } = false;

    [Display(Name = "Показывать элементы управления")]
    public bool VideoControls { get; set; } = false;
    [Display(Name = "Масштабирование")]
    public string ObjectFit { get; set; } = "cover";

    [Display(Name = "Положение")]
    public string ObjectPosition { get; set; } = "center";
}