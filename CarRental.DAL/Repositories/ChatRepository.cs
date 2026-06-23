using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.DAL.Data;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Repositories;

public class ChatRepository : Repository<Chat>, IChatRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ChatRepository(ApplicationDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<Chat?> GetByIdWithMessagesAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Chat>> GetActiveChatsAsync()
    {
        return await _dbSet
            .Include(c => c.User)
            .Where(c => c.Status == ChatStatus.Active || c.Status == ChatStatus.Waiting)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadChatsCountAsync()
    {
        return await _dbSet
            .CountAsync(c => c.UnreadCount > 0 && 
                           (c.Status == ChatStatus.Active || c.Status == ChatStatus.Waiting));
    }

    public async Task<bool> CloseChatAsync(Guid chatId)
    {
        var chat = await GetByIdAsync(chatId);
        if (chat == null) return false;

        chat.Status = ChatStatus.Closed;
        chat.UpdatedAt = DateTime.UtcNow;
        Update(chat);
        
        return await SaveChangesAsync();
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid chatId, Guid? adminId = null)
    {
        var chat = await GetByIdAsync(chatId);
        if (chat == null) return false;

        // Обновляем счетчик непрочитанных
        var unreadMessages = await _dbContext.ChatMessages
            .CountAsync(m => m.ChatId == chatId && !m.IsRead && m.MessageType == MessageType.User);
        
        chat.UnreadCount = unreadMessages;
        
        // Если сообщения читает админ, обновляем время последнего ответа
        if (adminId.HasValue)
            chat.LastAdminResponseAt = DateTime.UtcNow;
        
        chat.UpdatedAt = DateTime.UtcNow;
        Update(chat);
        
        return await SaveChangesAsync();
    }

    public async Task<IEnumerable<Chat>> GetExpiredTempChatsAsync(DateTime expiryDate)
    {
        return await _dbSet
            .Where(c => c.TempUserId != null &&
                        c.TempUserExpiry != null &&
                        c.TempUserExpiry <= expiryDate)
            .ToListAsync();
    }
    public async Task<Chat?> GetActiveChatByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && (c.Status == ChatStatus.Active || c.Status == ChatStatus.Waiting))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Chat?> GetLastClosedChatByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && c.Status == ChatStatus.Closed)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Chat?> GetLatestChatByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Chat?> GetLatestChatByTempUserIdAsync(Guid tempUserId)
    {
        return await _dbSet
            .Where(c => c.TempUserId == tempUserId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }
    public async Task<bool> DeleteChatAsync(Guid chatId)
    {
        var chat = await GetByIdAsync(chatId);
        if (chat == null) return false;
        Delete(chat);
        return await SaveChangesAsync();
    }
}