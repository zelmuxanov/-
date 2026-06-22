using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId);
    Task<IEnumerable<Booking>> GetBookingsByCarAsync(Guid carId);
    Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status);
    Task<bool> HasActiveBookingAsync(Guid userId);
    Task<IEnumerable<Booking>> GetRecentBookingsAsync(int count = 5);
    Task<IEnumerable<Booking>> GetBookingsForCarAsync(Guid carId);
    Task<IEnumerable<Booking>> GetExpiredConfirmedBookingsAsync(DateTime now);
    Task<Booking?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Booking>> GetAllBookingsAsync();     
    Task<IEnumerable<Booking>> GetBookingsForPeriodAsync(DateTime start, DateTime end);
}