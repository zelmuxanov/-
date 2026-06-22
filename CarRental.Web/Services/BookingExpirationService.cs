using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Services;

public class BookingExpirationService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BookingExpirationService> _logger;

    public BookingExpirationService(IServiceProvider services, ILogger<BookingExpirationService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                await bookingService.CompleteExpiredBookingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автоматическом завершении бронирований");
            }

            // Проверяем каждый час
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}