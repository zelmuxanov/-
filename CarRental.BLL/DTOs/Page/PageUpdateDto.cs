using System.ComponentModel.DataAnnotations;

namespace CarRental.BLL.DTOs.Page;

public class PageUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL-адрес (slug) обязателен")]
    [StringLength(200, ErrorMessage = "Slug не должен превышать 200 символов")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug может содержать только строчные латинские буквы, цифры и дефис")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Содержимое обязательно")]
    public string Content { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Meta-описание не должно превышать 300 символов")]
    public string? MetaDescription { get; set; }

    public bool IsActive { get; set; }

    [Range(0, 1000, ErrorMessage = "Порядок отображения должен быть от 0 до 1000")]
    public int DisplayOrder { get; set; }

    public DateTime? PublishedAt { get; set; }
}