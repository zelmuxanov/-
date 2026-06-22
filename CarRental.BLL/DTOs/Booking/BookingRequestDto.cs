using System.ComponentModel.DataAnnotations;

namespace CarRental.BLL.DTOs.Booking;

public class BookingRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid CarId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public decimal TotalPrice { get; set; }
    
    public string? Notes { get; set; }
    public decimal DepositAmount { get; set; } = 20000;
}