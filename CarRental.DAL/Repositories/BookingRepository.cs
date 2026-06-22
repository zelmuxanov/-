using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId)
    {
        return await _dbSet
            .Include(b => b.Car)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCarAsync(Guid carId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Where(b => b.CarId == carId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Car)
            .Where(b => b.Status == status)
            .ToListAsync();
    }

    public async Task<bool> HasActiveBookingAsync(Guid userId)
    {
        return await _dbSet.AnyAsync(b =>
            b.UserId == userId &&
            (b.Status == BookingStatus.Active ||
             b.Status == BookingStatus.Confirmed));
    }
    public async Task<Booking?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(b => b.Car)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public override async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(b => b.Car)
                .ThenInclude(c => c.Images)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
    public async Task<IEnumerable<Booking>> GetRecentBookingsAsync(int count = 5)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Car)
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    public async Task<IEnumerable<Booking>> GetBookingsForCarAsync(Guid carId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Where(b => b.CarId == carId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
            .ToListAsync();
    }
    public async Task<IEnumerable<Booking>> GetExpiredConfirmedBookingsAsync(DateTime now)
    {
        return await _dbSet
            .Where(b => b.Status == BookingStatus.Confirmed && b.EndDate < now)
            .ToListAsync();
    }
    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _dbSet
            .Include(b => b.Car).ThenInclude(c => c.Images)
            .Include(b => b.User)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsForPeriodAsync(DateTime start, DateTime end)
    {
        return await _dbSet
            .Include(b => b.Car).ThenInclude(c => c.Images)
            .Include(b => b.User)
            .Where(b => b.StartDate <= end && b.EndDate >= start)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }
}
