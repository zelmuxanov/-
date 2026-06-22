namespace CarRental.BLL.DTOs.Chat;

public class ChatCreateDto
{
    public Guid? UserId { get; set; }     
    public Guid? TempUserId { get; set; }     
    public string? TempUserName { get; set; }
    public string? TempUserEmail { get; set; }
    public string? TempUserPhone { get; set; }
    public string Topic { get; set; } = string.Empty;
}