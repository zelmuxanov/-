using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Admin;

public class PageCreateEditViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Введите заголовок")]
    [Display(Name = "Заголовок")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите URL (slug)")]
    [Display(Name = "URL (slug)")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Только строчные латинские буквы, цифры и дефис")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите содержимое")]
    [Display(Name = "Содержимое (HTML)")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Meta-описание")]
    [StringLength(300)]
    public string? MetaDescription { get; set; }

    [Display(Name = "Активна")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Порядок отображения")]
    [Range(0, 1000)]
    public int DisplayOrder { get; set; }

    [Display(Name = "Дата публикации")]
    public DateTime? PublishedAt { get; set; }
}