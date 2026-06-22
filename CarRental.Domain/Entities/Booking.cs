using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }
    public decimal DepositAmount { get; set; } = 20000;
    public string? ContractNumber { get; set; }        
    public string? ContractUrl { get; set; }             

    public virtual User? User { get; set; }
    public virtual Car Car { get; set; } = null!;
}