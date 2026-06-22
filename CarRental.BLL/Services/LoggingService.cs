using Microsoft.Extensions.Logging;

namespace CarRental.BLL.Services;

public class LoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation("{Timestamp} - {Message}", DateTime.Now, message);
    }

    public void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, "{Timestamp} - {Message}", DateTime.Now, message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning("{Timestamp} - {Message}", DateTime.Now, message);
    }
}