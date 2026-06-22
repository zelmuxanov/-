using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CarRental.Domain.Interfaces.Repositories;

namespace CarRental.BLL.Services;

public class ChatCleanupService : BackgroundService
{
    private readonly ILogger<ChatCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Проверка каждый час
    private readonly TimeSpan _tempChatLifetime = TimeSpan.FromHours(24); // Время жизни временных чатов

    public ChatCleanupService(ILogger<ChatCleanupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cleanup Service started. Will clean expired temp chats every hour.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Starting cleanup of expired temporary chats...");
                
                await CleanupExpiredTempChatsAsync();
                
                _logger.LogDebug("Cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cleanup of temporary chats");
            }

            // Ожидаем до следующего запуска
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Сервис остановлен
                break;
            }
        }

        _logger.LogInformation("Cleanup Service stopped.");
    }

    private async Task CleanupExpiredTempChatsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        
        var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();
        var chatMessageRepository = scope.ServiceProvider.GetRequiredService<IChatMessageRepository>();
        
        // Время, после которого чат считается просроченным
        var expiryDate = DateTime.UtcNow.AddHours(-24);
        
        // Получаем все просроченные временные чаты
        var expiredChats = await chatRepository.GetExpiredTempChatsAsync(expiryDate);
        
        _logger.LogInformation($"Found {expiredChats.Count()} expired temporary chats to cleanup");
        
        foreach (var chat in expiredChats)
        {
            try
            {
                _logger.LogDebug($"Cleaning up chat {chat.Id} (TempUser: {chat.TempUserId}) created at {chat.CreatedAt}");
                
                // Удаляем все сообщения чата
                var messages = await chatMessageRepository.GetMessagesByChatIdAsync(chat.Id);
                foreach (var message in messages)
                {
                    chatMessageRepository.Delete(message);
                }
                
                // Удаляем сам чат
                chatRepository.Delete(chat);
                
                _logger.LogDebug($"Chat {chat.Id} deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cleaning up chat {chat.Id}");

            }
        }
        

        await chatRepository.SaveChangesAsync();
        await chatMessageRepository.SaveChangesAsync();
        
        _logger.LogInformation($"Cleanup completed. Deleted {expiredChats.Count()} expired chats.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleanup Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}