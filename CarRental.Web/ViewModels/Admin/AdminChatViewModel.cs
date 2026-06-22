using CarRental.Web.ViewModels.Chat;

namespace CarRental.Web.ViewModels.Admin;

public class AdminChatViewModel
{
    public List<ChatListItemViewModel> Chats { get; set; } = new();
    public int UnreadChatsCount { get; set; }
    public ChatViewModel? CurrentChat { get; set; }
}

public class ChatListItemViewModel
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasUnread => UnreadCount > 0;
}