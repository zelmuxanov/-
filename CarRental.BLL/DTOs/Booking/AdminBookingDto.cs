using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Booking;

public class AdminBookingDto
{
    public Guid CarId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Notes { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
}