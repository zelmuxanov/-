using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IChatRepository : IRepository<Chat>
{
    Task<Chat?> GetByIdWithMessagesAsync(Guid id);
    Task<IEnumerable<Chat>> GetActiveChatsAsync();
    Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId);
    Task<int> GetUnreadChatsCountAsync();
    Task<bool> CloseChatAsync(Guid chatId);
    Task<bool> MarkMessagesAsReadAsync(Guid chatId, Guid? adminId = null);
    Task<IEnumerable<Chat>> GetExpiredTempChatsAsync(DateTime expiryDate);
    
    // Добавляем для улучшенной логики поиска чата
    Task<Chat?> GetActiveChatByUserIdAsync(Guid userId);
    Task<Chat?> GetLastClosedChatByUserIdAsync(Guid userId);
    Task<Chat?> GetLatestChatByUserIdAsync(Guid userId);       // переименовано/уточнено
    Task<Chat?> GetLatestChatByTempUserIdAsync(Guid tempUserId);
    Task<bool> DeleteChatAsync(Guid chatId);
}