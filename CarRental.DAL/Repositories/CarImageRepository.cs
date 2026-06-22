using CarRental.DAL.Data;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRental.DAL.Repositories;

public class CarImageRepository : Repository<CarImage>, ICarImageRepository
{
    public CarImageRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<CarImage>> GetByCarIdAsync(Guid carId)
    {
        return await _dbSet
            .Where(i => i.CarId == carId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();
    }

    public async Task<CarImage?> GetMainImageAsync(Guid carId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.CarId == carId && i.IsMain);
    }

    public async Task<bool> SetMainAsync(Guid imageId)
    {
        var image = await GetByIdAsync(imageId);
        if (image == null) return false;
        
        // Снимаем статус главного у всех изображений этого авто
        var others = await _dbSet.Where(i => i.CarId == image.CarId).ToListAsync();
        foreach (var img in others)
            img.IsMain = false;
        
        image.IsMain = true;
        await SaveChangesAsync();
        return true;
    }
    public async Task<int> CountByCarIdAsync(Guid carId)
    {
        return await _dbSet.CountAsync(i => i.CarId == carId);
    }
    public async Task UpdateDisplayOrderAsync(Guid imageId, int newOrder)
    {
        var image = await GetByIdAsync(imageId);
        if (image != null)
        {
            image.DisplayOrder = newOrder;
            Update(image);
            await SaveChangesAsync();
        }
    }
}