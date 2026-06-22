namespace CarRental.Web.ViewModels.Chat;

public class ChatMessageViewModel
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public bool IsUserMessage => MessageType == "User";
    public bool IsAdminMessage => MessageType == "Admin";
    public bool IsSystemMessage => MessageType == "System";
}