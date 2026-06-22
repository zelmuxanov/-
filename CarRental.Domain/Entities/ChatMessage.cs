using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatId { get; set; }
    public virtual Chat Chat { get; set; } = null!;
    
    public string Message { get; set; } = string.Empty;
    public MessageType MessageType { get; set; } = MessageType.User;
    
    // Для сообщений от пользователей
    public Guid? SenderUserId { get; set; }
    public virtual User? SenderUser { get; set; }
    
    // Для сообщений от администраторов
    public Guid? SenderAdminId { get; set; }
    public virtual User? SenderAdmin { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    // Вложения (если нужно расширить)
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    
    // Метод для получения имени отправителя
    public string GetSenderName()
    {
        return MessageType switch
        {
            MessageType.User => SenderUser != null 
                ? $"{SenderUser.FirstName} {SenderUser.LastName}"
                : "Гость",
            MessageType.Admin => SenderAdmin != null 
                ? $"Админ: {SenderAdmin.FirstName}"
                : "Администратор",
            MessageType.System => "Система",
            _ => "Неизвестный"
        };
    }
    
    // Метод для проверки, является ли отправитель пользователем
    public bool IsUserMessage() => MessageType == MessageType.User;
    public bool IsAdminMessage() => MessageType == MessageType.Admin;
}