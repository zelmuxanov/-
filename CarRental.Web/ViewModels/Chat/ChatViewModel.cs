namespace CarRental.Web.ViewModels.Chat;

public class ChatViewModel
{
    public Guid? ChatId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TempUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ChatMessageViewModel> Messages { get; set; } = new();
    public bool IsAdmin { get; set; }
    public string? TempUserName { get; set; }
    public string? TempUserEmail { get; set; }
    public string? TempUserPhone { get; set; }
    public string? ContactInfo { get; set; }
}