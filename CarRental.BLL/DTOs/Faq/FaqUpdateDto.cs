using System.ComponentModel.DataAnnotations;

namespace CarRental.BLL.DTOs.Faq;

public class FaqUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Вопрос обязателен")]
    [StringLength(500, ErrorMessage = "Вопрос не должен превышать 500 символов")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ответ обязателен")]
    public string Answer { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Категория не должна превышать 100 символов")]
    public string? Category { get; set; }

    [Range(0, 1000, ErrorMessage = "Порядок отображения должен быть от 0 до 1000")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}