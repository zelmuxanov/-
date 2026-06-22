using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Chat : BaseEntity
{
    // Для аутентифицированных пользователей
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }
    
    // Для неаутентифицированных (временный идентификатор)
    public Guid? TempUserId { get; set; }
    public string? TempUserName { get; set; }
    public string? TempUserEmail { get; set; }
    public string? TempUserPhone { get; set; }
    
    // Общая информация о чате
    public string Topic { get; set; } = string.Empty;
    public ChatStatus Status { get; set; } = ChatStatus.Active;
    public int UnreadCount { get; set; } = 0;
    public DateTime? LastMessageAt { get; set; }
    public DateTime? LastAdminResponseAt { get; set; }
    public DateTime? TempUserExpiry { get; set; } // Для временных пользователей (24 часа)
    
    // Навигационное свойство
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    
    // Метод для получения отображаемого имени
    public string GetDisplayName()
    {
        if (User != null)
            return $"{User.FirstName} {User.LastName}";
        
        return !string.IsNullOrEmpty(TempUserName) 
            ? TempUserName 
            : "Гость";
    }
    
    // Метод для получения контактной информации
    public string GetContactInfo()
    {
        if (User != null)
            return User.Email ?? User.PhoneNumber ?? "Нет контакта";
        
        return TempUserEmail ?? TempUserPhone ?? "Нет контакта";
    }
}