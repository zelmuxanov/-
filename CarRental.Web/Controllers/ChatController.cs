using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CarRental.Web.ViewModels.Chat;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Chat;
using System.Text.Json.Serialization;
using CarRental.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CarRental.Web.Controllers;

[Route("Chat")] // явный базовый маршрут
public class ChatController : Controller
{
    private readonly IChatService _chatService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ChatController> _logger;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(
        IChatService chatService,
        IFileStorageService fileStorageService,
        ILogger<ChatController> logger,
        IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _hubContext = hubContext;
    }

    // GET: Chat/Index (только для авторизованных)
    [HttpGet("Index")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var chat = await _chatService.GetOrCreateChatAsync(userId, null);
            if (chat == null) return NotFound();

            var messages = await _chatService.GetMessagesByChatIdAsync(chat.Id);
            var viewModel = new ChatViewModel
            {
                ChatId = chat.Id,
                UserId = userId,
                DisplayName = chat.DisplayName,
                Status = chat.Status.ToString(),
                ContactInfo = chat.ContactInfo, 
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
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading chat");
            return View("Error", new ErrorViewModel { Message = "Ошибка при загрузке чата" });
        }
    }

    // GET: Chat/GetOrCreate (для анонимных пользователей)
    [AllowAnonymous]
    [HttpGet("GetOrCreate")]
    public async Task<IActionResult> GetOrCreate(Guid? userId = null, Guid? tempUserId = null, string? tempUserName = null)
    {
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = GetCurrentUserId();
                tempUserId = null;
            }

            if (!User.Identity?.IsAuthenticated == true && !tempUserId.HasValue)
                tempUserId = Guid.NewGuid();

            var chat = await _chatService.GetOrCreateChatAsync(userId, tempUserId, tempUserName);

            if (chat == null)
                return Json(new { success = false, error = "Не удалось создать чат" });

            if (!User.Identity?.IsAuthenticated == true && tempUserId.HasValue)
            {
                Response.Cookies.Append("ChatTempUserId", tempUserId.Value.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(24),
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                });
            }

            return Json(new { success = true, chatId = chat.Id, tempUserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/loading chat");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // GET: Chat/Messages/{chatId} (для всех)
    [HttpGet("Messages")]
    [AllowAnonymous]
    public async Task<IActionResult> Messages(Guid chatId)
    {
        try
        {
            var chat = await _chatService.GetChatByIdAsync(chatId);
            if (chat == null)
                return Json(new { error = "Чат не найден" });

            // Проверка доступа
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetCurrentUserId();
                if (chat.UserId != userId && !User.IsInRole("Admin"))
                    return Json(new { error = "Доступ запрещен" });
            }
            else
            {
                var tempUserId = Request.Cookies["ChatTempUserId"];
                if (string.IsNullOrEmpty(tempUserId) || chat.TempUserId?.ToString() != tempUserId)
                    return Json(new { error = "Доступ запрещен" });
            }

            var messages = await _chatService.GetMessagesByChatIdAsync(chatId);
            return Json(new { messages });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading messages");
            return Json(new { error = "Ошибка при загрузке сообщений" });
        }
    }

    // POST: Chat/SendMessage (JSON, без файлов)
    [HttpPost("SendMessage")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return Json(new { success = false, error = "Сообщение не может быть пустым" });

            ChatDto? chat = null;
            Guid? userId = null;
            Guid? tempUserId = null;
            Guid? adminId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    adminId = GetCurrentUserId();
                }
                else
                {
                    userId = GetCurrentUserId();
                }
            }
            else if (request.TempUserId.HasValue)
            {
                tempUserId = request.TempUserId;
            }
            else
            {
                return Json(new { success = false, error = "Неизвестный отправитель" });
            }

            if (request.ChatId.HasValue)
            {
                chat = await _chatService.GetChatByIdAsync(request.ChatId.Value);
                if (chat == null)
                    return Json(new { success = false, error = "Чат не найден" });
            }
            else
            {
                if (adminId.HasValue)
                    return Json(new { success = false, error = "Администратор должен указать chatId" });

                chat = await _chatService.GetOrCreateChatAsync(userId, tempUserId, request.TempUserName);
                if (chat == null)
                    return Json(new { success = false, error = "Не удалось создать чат" });
            }

            var messageDto = new ChatMessageCreateDto
            {
                ChatId = chat.Id,
                Message = request.Message,
                UserId = userId,
                TempUserId = tempUserId,
                AdminId = adminId
            };

            var result = await _chatService.SendMessageAsync(messageDto);
            return Json(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // POST: Chat/SendMessageWithAttachment (multipart/form-data, с файлом)
    [HttpPost("SendMessageWithAttachment")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessageWithAttachment(
        [FromForm] Guid? chatId,
        [FromForm] string message,
        [FromForm] Guid? tempUserId,
        [FromForm] string? tempUserName,
        IFormFile? attachment)
    {
        try
        {
            // Проверка: должно быть хоть что-то
            if (string.IsNullOrWhiteSpace(message) && attachment == null)
                return Json(new { success = false, error = "Нет текста или файла" });

            Guid? userId = null;
            Guid? adminId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    adminId = GetCurrentUserId();
                else
                    userId = GetCurrentUserId();
            }
            else if (tempUserId.HasValue)
            {
                // уже есть tempUserId
            }
            else
            {
                return Json(new { success = false, error = "Неизвестный отправитель" });
            }

            ChatDto? chat = null;
            if (chatId.HasValue)
            {
                chat = await _chatService.GetChatByIdAsync(chatId.Value);
                if (chat == null)
                    return Json(new { success = false, error = "Чат не найден" });
            }
            else
            {
                chat = await _chatService.GetOrCreateChatAsync(userId, tempUserId, tempUserName);
                if (chat == null)
                    return Json(new { success = false, error = "Не удалось создать чат" });
            }

            string? attachmentUrl = null;
            string? attachmentType = null;

            if (attachment != null && attachment.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(attachment.ContentType))
                    return Json(new { success = false, error = "Можно загружать только изображения" });

                attachmentUrl = await _fileStorageService.SaveFileAsync(attachment, "chat");
                attachmentType = attachment.ContentType;
            }

            var messageDto = new ChatMessageCreateDto
            {
                ChatId = chat.Id,
                Message = message ?? "",
                UserId = userId,
                TempUserId = tempUserId,
                AdminId = adminId,
                AttachmentUrl = attachmentUrl,
                AttachmentType = attachmentType
            };

            var result = await _chatService.SendMessageAsync(messageDto);
            await _hubContext.Clients.Group(chat.Id.ToString()).SendAsync("ReceiveMessage", result);
            return Json(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message with attachment");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("MarkAsRead")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var adminId = User.IsInRole("Admin") ? userId : (Guid?)null;
            
            await _chatService.MarkMessagesAsReadAsync(request.ChatId, adminId);
            
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
            return Json(new { success = false, error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User identifier claim is missing.");
        }
        return Guid.Parse(userIdClaim);
    }

    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public Guid? ChatId { get; set; }
        public Guid? TempUserId { get; set; }
        public string? TempUserName { get; set; }
    }

    public class MarkAsReadRequest
    {
        public Guid ChatId { get; set; }
    }
}

public class ErrorViewModel
{
    public string? Message { get; set; }
}