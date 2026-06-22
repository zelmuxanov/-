namespace CarRental.BLL.DTOs.Chat;

public class ChatMessageCreateDto
{
    public Guid ChatId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? TempUserId { get; set; }
    public Guid? AdminId { get; set; }

    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
}