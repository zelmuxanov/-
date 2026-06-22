using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.Web.ViewModels.Chat;
using CarRental.Web.ViewModels.Admin;
using CarRental.BLL.Interfaces.Services;
using System.Text.Json.Serialization;
using CarRental.Web.Services;
using CarRental.BLL.DTOs.Chat;

namespace CarRental.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ChatController : BaseAdminController
{
    private readonly IChatService _chatService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserService _userService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, IFileStorageService fileStorageService, IUserService userService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _fileStorageService = fileStorageService;
        _userService = userService;
        _logger = logger;
    }

    // GET: Admin/Chat
    [HttpGet]
    public async Task<IActionResult> Index(Guid? chatId = null)
    {
        var chats = await _chatService.GetActiveChatsAsync();
        var viewModel = new AdminChatViewModel
        {
            Chats = chats.Select(c => new ChatListItemViewModel
            {
                Id = c.Id,
                DisplayName = c.DisplayName,
                ContactInfo = c.ContactInfo,
                Status = c.Status.ToString(),
                UnreadCount = c.UnreadCount,
                LastMessageAt = c.LastMessageAt,
                CreatedAt = c.CreatedAt
            }).OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt).ToList(),
            UnreadChatsCount = await _chatService.GetUnreadChatsCountAsync()
        };

        if (chatId.HasValue)
        {
            var chat = await _chatService.GetChatByIdAsync(chatId.Value);
            if (chat != null)
            {
                var messages = await _chatService.GetMessagesByChatIdAsync(chatId.Value);
                viewModel.CurrentChat = new ChatViewModel
                {
                    ChatId = chat.Id,
                    DisplayName = chat.DisplayName,
                    Status = chat.Status.ToString(),
                    Messages = messages.Select(m => new ChatMessageViewModel
                    {
                        Id = m.Id,
                        ChatId = m.ChatId,
                        Message = m.Message,
                        MessageType = m.MessageType.ToString(),
                        SenderName = m.SenderName,
                        CreatedAt = m.CreatedAt,
                        IsRead = m.IsRead,
                        AttachmentUrl = m.AttachmentUrl,
                        AttachmentType = m.AttachmentType  
                    }).ToList(),
                    IsAdmin = true,
                    ContactInfo = chat.ContactInfo 
                };
            }
        }

        return View(viewModel);
    }

    // POST: Admin/Chat/Close/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id)
    {
        try
        {
            var result = await _chatService.CloseChatAsync(id);
            
            if (result)
                return Json(new { success = true });
            else
                return Json(new { success = false, error = "Не удалось закрыть чат" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing chat");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // GET: Admin/Chat/GetChat/{id}
    [HttpGet]
    public async Task<IActionResult> GetChat(Guid id)
    {
        try
        {
            var chat = await _chatService.GetChatByIdAsync(id);
            if (chat == null)
                return Json(new { success = false, error = "Чат не найден" });

            var messages = await _chatService.GetMessagesByChatIdAsync(id);

            return Json(new
            {
                success = true,
                chat = new
                {
                    chat.Id,
                    chat.DisplayName,
                    chat.ContactInfo,
                    chat.Status,
                    chat.UnreadCount,
                    messages = messages.Select(m => new
                    {
                        m.Id,
                        m.Message,
                        m.MessageType,
                        m.SenderName,
                        m.CreatedAt,
                        m.IsRead
                    })
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteChat([FromBody] DeleteChatRequest request)
    {
        try
        {
            var result = await _chatService.DeleteChatAsync(request.ChatId);
            if (result)
                return Json(new { success = true });
            return Json(new { success = false, error = "Чат не найден" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat");
            return Json(new { success = false, error = ex.Message });
        }
    }

    public class DeleteChatRequest
    {
        [JsonPropertyName("chatId")]
        public Guid ChatId { get; set; }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessageWithAttachment(
        [FromForm] Guid chatId,
        [FromForm] string message,
        IFormFile? attachment)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message) && attachment == null)
                return Json(new { success = false, error = "Нет текста или файла" });

            var adminId = GetCurrentUserId();

            string? attachmentUrl = null;
            string? attachmentType = null;

            if (attachment != null && attachment.Length > 0)
            {
                // Разрешаем только изображения
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(attachment.ContentType))
                    return Json(new { success = false, error = "Можно загружать только изображения" });

                attachmentUrl = await _fileStorageService.SaveFileAsync(attachment, "chat");
                attachmentType = attachment.ContentType;
            }

            var messageDto = new ChatMessageCreateDto
            {
                ChatId = chatId,
                Message = message ?? "",
                AdminId = adminId,
                AttachmentUrl = attachmentUrl,
                AttachmentType = attachmentType
            };

            var result = await _chatService.SendMessageAsync(messageDto);
            return Json(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message with attachment");
            return Json(new { success = false, error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User identifier claim is missing.");
        return Guid.Parse(userIdClaim);
    }
    [HttpPost("CreateChatForUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChatForUser(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        var chat = await _chatService.GetOrCreateChatAsync(userId, null, $"{user.FirstName} {user.LastName}");
        return RedirectToAction(nameof(Index), new { chatId = chat?.Id });
    }
}