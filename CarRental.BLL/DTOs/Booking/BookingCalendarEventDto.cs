namespace CarRental.BLL.DTOs.Booking;

public class BookingCalendarEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid CarId { get; set; }
    public string CarInfo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}