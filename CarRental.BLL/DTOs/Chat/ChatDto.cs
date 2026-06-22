using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Chat;

public class ChatDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TempUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public ChatStatus Status { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime? LastAdminResponseAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}