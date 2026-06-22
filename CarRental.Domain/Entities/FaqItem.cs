using System.ComponentModel.DataAnnotations;

namespace CarRental.Domain.Entities;

public class FaqItem : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty; // Длинный текст

    [MaxLength(100)]
    public string? Category { get; set; }               // Категория вопроса

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}