using CarRental.Domain.Entities;
using CarRental.BLL.DTOs.Booking; 

namespace CarRental.BLL.Interfaces.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task<bool> SendEmailConfirmationAsync(User user, string confirmationToken, string confirmationCode);
    Task<bool> SendPasswordResetAsync(User user, string resetToken, string? resetCode = null); // Один метод с необязательным параметром
    Task<bool> SendManagerNotificationAsync(string subject, string message);
    Task<bool> SendBookingConfirmationAsync(Booking booking, User user);
    Task<bool> SendBookingCancellationAsync(Booking booking, User user);
    Task<bool> SendNewUserNotificationAsync(User user);
    Task<bool> SendContractToClientAsync(BookingDto booking, string contractUrl);
}