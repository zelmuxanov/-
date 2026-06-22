using CarRental.Domain.Enums;

namespace CarRental.BLL.DTOs.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string Message { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid? SenderUserId { get; set; }
    public Guid? SenderAdminId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
}