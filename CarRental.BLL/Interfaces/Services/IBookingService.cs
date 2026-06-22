using CarRental.BLL.DTOs.Booking;
using CarRental.Domain.Enums;

namespace CarRental.BLL.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDto?> GetBookingByIdAsync(Guid id);
    Task<IEnumerable<BookingDto>> GetRecentBookingsAsync(int count = 5);
    Task<IEnumerable<BookingDto>> GetUserBookingsAsync(Guid userId);
    Task<IEnumerable<BookingDto>> GetBookingsByStatusAsync(BookingStatus status);
    Task<BookingDto> CreateBookingAsync(BookingRequestDto requestDto);
    Task<bool> CancelBookingAsync(Guid bookingId);
    Task<bool> ConfirmBookingAsync(Guid bookingId);
    Task<BookingCalculationDto> CalculateBookingPriceAsync(BookingRequestDto requestDto);
    Task<bool> HasActiveBookingAsync(Guid userId);
    Task<IEnumerable<BookingDto>> GetBookingsForCarAsync(Guid carId);
    Task CompleteExpiredBookingsAsync();
    Task UpdateBookingAsync(BookingDto bookingDto);
    Task<IEnumerable<BookingCalendarEventDto>> GetBookingsForCalendarAsync(DateTime? start = null, DateTime? end = null);
    Task<BookingDto> CreateAdminBookingAsync(AdminBookingDto adminDto);
    Task<bool> DeleteBookingAsync(Guid bookingId);
}
