using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CarRental.BLL.DTOs.Chat;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.BLL.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ChatService> _logger;
    private readonly IMapper _mapper;

    public ChatService(
        IChatRepository chatRepository,
        IChatMessageRepository chatMessageRepository,
        UserManager<User> userManager,
        ILogger<ChatService> logger,
        IMapper mapper)
    {
        _chatRepository = chatRepository;
        _chatMessageRepository = chatMessageRepository;
        _userManager = userManager;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ChatDto>> GetActiveChatsAsync()
    {
        var chats = await _chatRepository.GetActiveChatsAsync();
        return _mapper.Map<IEnumerable<ChatDto>>(chats);
    }

    public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(Guid userId)
    {
        var chats = await _chatRepository.GetUserChatsAsync(userId);
        return _mapper.Map<IEnumerable<ChatDto>>(chats);
    }

    public async Task<ChatDto?> GetChatByIdAsync(Guid id)
    {
        var chat = await _chatRepository.GetByIdWithMessagesAsync(id);
        return _mapper.Map<ChatDto?>(chat);
    }

    public async Task<ChatDto?> GetOrCreateChatAsync(
        Guid? userId,
        Guid? tempUserId,
        string? tempUserName = null,
        string? tempUserEmail = null,
        string? tempUserPhone = null)
    {
        Chat? existingChat = null;

        if (userId.HasValue)
        {
            existingChat = await _chatRepository.GetActiveChatByUserIdAsync(userId.Value);
            if (existingChat != null)
                return _mapper.Map<ChatDto>(existingChat);

            existingChat = await _chatRepository.GetLastClosedChatByUserIdAsync(userId.Value);
            if (existingChat != null)
            {
                existingChat.Status = ChatStatus.Active;
                existingChat.UpdatedAt = DateTime.UtcNow;
                _chatRepository.Update(existingChat);
                await _chatRepository.SaveChangesAsync();
                return _mapper.Map<ChatDto>(existingChat);
            }
        }
        else if (tempUserId.HasValue)
        {
            existingChat = await _chatRepository.GetLatestChatByTempUserIdAsync(tempUserId.Value);
            if (existingChat != null)
            {
                if (existingChat.Status != ChatStatus.Active && existingChat.Status != ChatStatus.Waiting)
                {
                    existingChat.Status = ChatStatus.Active;
                    existingChat.UpdatedAt = DateTime.UtcNow;
                    _chatRepository.Update(existingChat);
                    await _chatRepository.SaveChangesAsync();
                }
                return _mapper.Map<ChatDto>(existingChat);
            }
        }

        var chat = new Chat
        {
            UserId = userId,
            TempUserId = tempUserId,
            TempUserName = tempUserName ?? (tempUserId.HasValue ? "Гость" : null),
            TempUserEmail = tempUserEmail,
            TempUserPhone = tempUserPhone,
            Topic = "Общий вопрос",
            Status = ChatStatus.Active,
            TempUserExpiry = tempUserId.HasValue ? DateTime.UtcNow.AddHours(24) : null,
            UnreadCount = 0
        };

        await _chatRepository.AddAsync(chat);
        await _chatRepository.SaveChangesAsync();

        var welcomeMessage = new ChatMessage
        {
            ChatId = chat.Id,
            Message = "Здравствуйте! Чем мы можем вам помочь?\n\n" +
                 "Вы можете задать любой вопрос по аренде автомобилей. Если у вас возникли трудности, " +
                 "Возможно, ответ на ваш вопрос уже есть в разделе часто задаваемых вопросов " +
                 "https://o-prokat.ru/Home/FAQ\n\n" +
                 "Служба поддержки ответит вам в ближайшее время.",
            MessageType = MessageType.System,
            CreatedAt = DateTime.UtcNow
        };
        await _chatMessageRepository.AddAsync(welcomeMessage);
        await _chatMessageRepository.SaveChangesAsync();

        return _mapper.Map<ChatDto>(chat);
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesByChatIdAsync(Guid chatId)
    {
        var messages = await _chatMessageRepository.GetMessagesByChatIdAsync(chatId);
        return _mapper.Map<IEnumerable<ChatMessageDto>>(messages);
    }

    public async Task<ChatMessageDto> SendMessageAsync(ChatMessageCreateDto dto)
    {
        var chat = await _chatRepository.GetByIdWithMessagesAsync(dto.ChatId);
        if (chat == null)
            throw new KeyNotFoundException($"Chat with id {dto.ChatId} not found");

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = dto.ChatId,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            AttachmentUrl = dto.AttachmentUrl,    // новая строка
            AttachmentType = dto.AttachmentType
        };

        // Определяем тип отправителя
        if (dto.AdminId.HasValue)
        {
            message.MessageType = MessageType.Admin;
            message.SenderAdminId = dto.AdminId;
            chat.LastAdminResponseAt = DateTime.UtcNow;
            // НЕ увеличиваем UnreadCount для админа
        }
        else if (dto.UserId.HasValue)
        {
            message.MessageType = MessageType.User;
            message.SenderUserId = dto.UserId;
            chat.UnreadCount += 1;
        }
        else if (dto.TempUserId.HasValue)
        {
            message.MessageType = MessageType.User;
            message.SenderUserId = null;
            if (chat.TempUserId != dto.TempUserId)
                throw new UnauthorizedAccessException("Chat does not belong to this guest");
            
            chat.UnreadCount += 1;
        }
        else
        {
            throw new ArgumentException("Invalid sender information");
        }

        chat.LastMessageAt = DateTime.UtcNow;
        chat.UpdatedAt = DateTime.UtcNow;

        await _chatMessageRepository.AddAsync(message);
        _chatRepository.Update(chat);
        await _chatRepository.SaveChangesAsync();

        bool isFirstUserMessage = !(await _chatMessageRepository.GetMessagesByChatIdAsync(chat.Id))
                                        .Any(m => m.MessageType == MessageType.User);

        
        var dtoResult = _mapper.Map<ChatMessageDto>(message);
        dtoResult.SenderName = await GetSenderNameAsync(message, chat);
        return dtoResult;
    }

    private readonly Dictionary<Guid, string> _userNameCache = new();
    private readonly Dictionary<Guid, string> _adminNameCache = new();

    private async Task<string> GetSenderNameAsync(ChatMessage message, Chat chat)
    {
        if (message.MessageType == MessageType.Admin && message.SenderAdminId.HasValue)
        {
            var adminId = message.SenderAdminId.Value;
            if (_adminNameCache.TryGetValue(adminId, out var cachedName))
                return cachedName;
            
            var admin = await _userManager.FindByIdAsync(adminId.ToString());
            var name = admin != null ? $"Админ: {admin.FirstName}" : "Администратор";
            _adminNameCache[adminId] = name;
            return name;
        }
        if (message.MessageType == MessageType.User && message.SenderUserId.HasValue)
        {
            var userId = message.SenderUserId.Value;
            if (_userNameCache.TryGetValue(userId, out var cachedName))
                return cachedName;
            
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var name = user != null ? $"{user.FirstName} {user.LastName}" : "Пользователь";
            _userNameCache[userId] = name;
            return name;
        }
        if (message.MessageType == MessageType.User && !message.SenderUserId.HasValue)
        {
            return !string.IsNullOrEmpty(chat.TempUserName) ? chat.TempUserName : "Гость";
        }
        return "Система";
    }

    public async Task<bool> CloseChatAsync(Guid chatId)
    {
        return await _chatRepository.CloseChatAsync(chatId);
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid chatId, Guid? adminId = null)
    {
        await _chatMessageRepository.MarkAllAsReadAsync(chatId, adminId);
        return await _chatRepository.MarkMessagesAsReadAsync(chatId, adminId);
    }

    public async Task<int> GetUnreadChatsCountAsync()
    {
        return await _chatRepository.GetUnreadChatsCountAsync();
    }

    public async Task<int> CleanupExpiredTempChatsAsync(DateTime expiryDate)
    {
        var expiredChats = await _chatRepository.GetExpiredTempChatsAsync(expiryDate);
        int count = 0;

        foreach (var chat in expiredChats)
        {
            _chatRepository.Delete(chat);
            count++;
        }

        if (count > 0)
            await _chatRepository.SaveChangesAsync();

        _logger.LogInformation("Очищено {Count} просроченных временных чатов", count);
        return count;
    }

    public async Task<bool> DeleteChatAsync(Guid chatId)
    {
        return await _chatRepository.DeleteChatAsync(chatId);
    }
    public async Task<ChatDto?> GetChatByUserIdAsync(Guid userId)
    {
        var chat = await _chatRepository.GetActiveChatByUserIdAsync(userId);
        return _mapper.Map<ChatDto?>(chat);
    }
}