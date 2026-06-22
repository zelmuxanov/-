using System.ComponentModel.DataAnnotations;

namespace CarRental.Domain.Entities;

public class Page : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty; // Уникальный URL-идентификатор

    public string Content { get; set; } = string.Empty; // HTML-контент

    [MaxLength(300)]
    public string? MetaDescription { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime? PublishedAt { get; set; }
}