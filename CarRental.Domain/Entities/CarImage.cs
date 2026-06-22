namespace CarRental.Domain.Entities;

public class CarImage : BaseEntity
{
    public Guid CarId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
    
    // Навигационное свойство
    public virtual Car Car { get; set; } = null!;
}