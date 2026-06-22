using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface ICarImageRepository : IRepository<CarImage>
{
    Task<IEnumerable<CarImage>> GetByCarIdAsync(Guid carId);
    Task<CarImage?> GetMainImageAsync(Guid carId);
    Task<bool> SetMainAsync(Guid imageId);
    Task<int> CountByCarIdAsync(Guid carId);
    Task UpdateDisplayOrderAsync(Guid imageId, int newOrder);
}