using CarRental.DAL.Data;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRental.DAL.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.Where(d => d.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetPendingAsync()
    {
        return await _dbSet.Where(d => d.Status == "Pending").ToListAsync();
    }

    public async Task<Document?> GetByIdWithUserAsync(Guid id)
    {
        return await _dbSet.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
    }
    public async Task<IEnumerable<Document>> GetByUserIdWithUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(d => d.User)
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetPendingWithUserAsync()
    {
        return await _dbSet
            .Include(d => d.User)
            .Where(d => d.Status == "Pending")
            .ToListAsync();
    }
}