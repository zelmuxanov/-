using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CarRental.BLL.DTOs.Chat;
using CarRental.BLL.Interfaces.Services;
using System.Security.Claims;

namespace CarRental.Web.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        _logger.LogInformation($"User {Context.ConnectionId} joined chat {chatId}");
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        _logger.LogInformation($"User {Context.ConnectionId} left chat {chatId}");
    }

    [Authorize(Roles = "Admin")]
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        _logger.LogInformation($"Admin {Context.User?.Identity?.Name} joined admin group");
    }

    [Authorize(Roles = "Admin")]
    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        _logger.LogInformation($"Admin {Context.User?.Identity?.Name} left admin group");
    }

    // Универсальный метод отправки сообщения
    public async Task SendMessage(Guid chatId, string message, Guid? userId, Guid? adminId, Guid? tempUserId = null)
    {
        try
        {
            var messageDto = new ChatMessageCreateDto
            {
                ChatId = chatId,
                Message = message,
                UserId = userId,
                AdminId = adminId,
                TempUserId = tempUserId
            };

            var result = await _chatService.SendMessageAsync(messageDto);
            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message via SignalR");
            await Clients.Caller.SendAsync("Error", "Ошибка при отправке сообщения");
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}