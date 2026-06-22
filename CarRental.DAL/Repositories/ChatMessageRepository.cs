using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ChatMessageRepository(ApplicationDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(Guid chatId)
    {
        return await _dbSet
            .Include(m => m.SenderUser)
            .Include(m => m.SenderAdmin)
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatMessage?> GetLastMessageByChatIdAsync(Guid chatId)
    {
        return await _dbSet
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetUnreadMessagesCountAsync(Guid chatId, bool forAdmin = true)
    {
        if (forAdmin)
        {
            // Сообщения от пользователя, не прочитанные админом
            return await _dbSet
                .CountAsync(m => m.ChatId == chatId && 
                               !m.IsRead && 
                               m.MessageType == MessageType.User);
        }
        else
        {
            // Сообщения от админа, не прочитанные пользователем
            return await _dbSet
                .CountAsync(m => m.ChatId == chatId && 
                               !m.IsRead && 
                               m.MessageType == MessageType.Admin);
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid messageId)
    {
        var message = await GetByIdAsync(messageId);
        if (message == null) return false;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        Update(message);
        
        return await SaveChangesAsync();
    }

    public async Task<bool> MarkAllAsReadAsync(Guid chatId, Guid? readerId = null)
    {
        var messages = await _dbSet
            .Where(m => m.ChatId == chatId && !m.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            _dbContext.Entry(message).State = EntityState.Modified;
        }
        
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50)
    {
        return await _dbSet
            .Include(m => m.Chat)
            .Include(m => m.SenderUser)
            .Include(m => m.SenderAdmin)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}