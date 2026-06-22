using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces.Repositories;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(Guid chatId);
    Task<ChatMessage?> GetLastMessageByChatIdAsync(Guid chatId);
    Task<int> GetUnreadMessagesCountAsync(Guid chatId, bool forAdmin = true);
    Task<bool> MarkAsReadAsync(Guid messageId);
    Task<bool> MarkAllAsReadAsync(Guid chatId, Guid? readerId = null);
    Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50);
}