namespace CarRental.BLL.DTOs.Booking;

public class BookingCalculationDto
{
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DepositAmount { get; set; } = 20000;
    public decimal TotalWithDeposit => TotalPrice + DepositAmount;
}