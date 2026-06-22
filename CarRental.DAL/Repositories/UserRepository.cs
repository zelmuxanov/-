using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class UserRepository : IUserRepository
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<User> _dbSet;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<User>();
    }

    public virtual async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public virtual async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<bool> UserExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public virtual async Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status)
    {
        return await _dbSet.Where(u => u.Status == status).ToListAsync();
    }

    public virtual async Task AddAsync(User entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(User entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(User entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
    public virtual async Task<int> GetUsersCountAsync()
    {
        return await _dbSet.CountAsync();
    }
}