using System.ComponentModel.DataAnnotations;

namespace CarRental.Domain.Entities;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
     protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow; // ВСЕГДА UTC для PostgreSQL
    }
}
