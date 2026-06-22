using CarRental.BLL.DTOs.Chat;

namespace CarRental.BLL.Interfaces.Services;

public interface IChatService
{
    // Получение чатов
    Task<IEnumerable<ChatDto>> GetActiveChatsAsync();
    Task<IEnumerable<ChatDto>> GetUserChatsAsync(Guid userId);
    Task<ChatDto?> GetChatByIdAsync(Guid id);
    Task<ChatDto?> GetChatByUserIdAsync(Guid userId);
    Task<ChatDto?> GetOrCreateChatAsync(Guid? userId, Guid? tempUserId, string? tempUserName = null, string? tempUserEmail = null, string? tempUserPhone = null);
    
    // Работа с сообщениями
    Task<IEnumerable<ChatMessageDto>> GetMessagesByChatIdAsync(Guid chatId);
    Task<ChatMessageDto> SendMessageAsync(ChatMessageCreateDto dto);
    
    // Административные функции
    Task<bool> CloseChatAsync(Guid chatId);
    Task<bool> MarkMessagesAsReadAsync(Guid chatId, Guid? adminId = null);
    Task<int> GetUnreadChatsCountAsync();
    Task<bool> DeleteChatAsync(Guid chatId);
    Task<int> CleanupExpiredTempChatsAsync(DateTime expiryDate);
}