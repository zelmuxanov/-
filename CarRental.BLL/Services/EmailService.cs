using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.Options;
using CarRental.Domain.Entities;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using CarRental.BLL.DTOs.Booking;

namespace CarRental.BLL.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly BookingEmailOptions _bookingEmailOptions;
    private readonly ManagerOptions _managerOptions;
    private readonly ILogger<EmailService> _logger;
    private readonly string _baseUrl;

    public EmailService(
        IOptions<EmailOptions> emailOptions,
        IOptions<BookingEmailOptions> bookingEmailOptions,
        IOptions<ManagerOptions> managerOptions,
        ILogger<EmailService> logger,
        IConfiguration configuration)
    {
        _emailOptions = emailOptions.Value;
        _bookingEmailOptions = bookingEmailOptions.Value;
        _managerOptions = managerOptions.Value;
        _logger = logger;
        _baseUrl = configuration["SiteSettings:BaseUrl"] ?? "https://o-prokat.ru";
    }

    private async Task<bool> SendEmailInternalAsync(
        string toEmail, 
        string subject, 
        string body, 
        ISmtpOptions options,
        bool isHtml = true)
    {
        try
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                _logger.LogWarning("Attempt to send email to empty address");
                return false;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(options.SenderName, options.SenderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
                var plainText = System.Text.RegularExpressions.Regex.Replace(body, "<[^>]*>", "");
                bodyBuilder.TextBody = plainText;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            SecureSocketOptions sslOption;
            if (options.SmtpPort == 465)
            {
                sslOption = SecureSocketOptions.SslOnConnect;
            }
            else if (options.SmtpPort == 587)
            {
                sslOption = SecureSocketOptions.StartTls;
            }
            else
            {
                sslOption = options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            }
            
            smtp.Timeout = 30000; // 30 секунд
            
            await smtp.ConnectAsync(options.SmtpServer, options.SmtpPort, sslOption);
            await smtp.AuthenticateAsync(options.Username, options.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        return await SendEmailInternalAsync(toEmail, subject, body, _emailOptions, isHtml);
    }

    public async Task<bool> SendEmailConfirmationAsync(User user, string confirmationToken, string confirmationCode)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("User {UserId} has no email for confirmation", user.Id);
            return false;
        }

        var confirmationLink = $"{_baseUrl}/EmailConfirmation/ConfirmEmail?" +
                        $"userId={Uri.EscapeDataString(user.Id.ToString())}&" +
                        $"token={Uri.EscapeDataString(confirmationToken)}";
        
        var subject = "Подтверждение email - О! Прокат";
        var body = $@"
            <h2>Здравствуйте, {user.FirstName}!</h2>
            <p>Спасибо за регистрацию в сервисе аренды автомобилей О! Прокат.</p>
            
            <div style='background: #f8f9fa; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                <h3>🔐 Способ 1: Используйте код</h3>
                <p><strong>Ваш код подтверждения:</strong></p>
                <div style='font-size: 28px; font-weight: bold; color: #2e7d32; padding: 15px; background: #e8f5e9; text-align: center; border-radius: 5px; margin: 15px 0; letter-spacing: 5px;'>
                    {confirmationCode}
                </div>
                <p>Введите этот 6-значный код на сайте для подтверждения email.</p>
                
                <hr style='margin: 25px 0;'>
                
                <h3>🔗 Способ 2: Перейдите по ссылке</h3>
                <p><a href='{confirmationLink}' style='display: inline-block; padding: 12px 24px; background: #2e7d32; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Подтвердить Email</a></p>
                <p><small>Или скопируйте ссылку: {confirmationLink}</small></p>
            </div>
            
            <div style='margin-top: 25px; padding: 15px; background: #fff3cd; border-radius: 5px;'>
                <p><strong>⚠️ Важно:</strong></p>
                <ul>
                    <li>Код и ссылка действительны <strong>24 часа</strong></li>
                    <li>Если вы не регистрировались в О! Прокат, проигнорируйте это письмо</li>
                    <li>Никому не сообщайте код</li>
                </ul>
            </div>
            
            <p>С уважением,<br>Команда О! Прокат</p>";

        return await SendEmailInternalAsync(user.Email, subject, body, _emailOptions, true);
    }

    public async Task<bool> SendPasswordResetAsync(User user, string resetToken, string? resetCode = null)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("User {UserId} has no email for password reset", user.Id);
            return false;
        }

        var resetLink = $"{_baseUrl}/PasswordRecovery/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
        
        var subject = "Восстановление доступа - О! Прокат";
        
        string body;
        
        // ВСЕГДА отправляем и ссылку, и код
        body = $@"
            <h2>Здравствуйте, {user.FirstName}!</h2>
            <p>Для восстановления доступа к вашему аккаунту в О! Прокат используйте код или ссылку ниже.</p>
            
            <div style='background: #f8f9fa; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                <h3>🔐 Способ 1: Используйте код</h3>
                <p><strong>Ваш код подтверждения:</strong></p>
                <div style='font-size: 28px; font-weight: bold; color: #2e7d32; padding: 15px; background: #e8f5e9; text-align: center; border-radius: 5px; margin: 15px 0; letter-spacing: 5px;'>
                    {resetCode}
                </div>
                <p>Перейдите на сайт и введите этот 6-значный код</p>
                
                <hr style='margin: 25px 0;'>
                
                <h3>🔗 Способ 2: Используйте ссылку</h3>
                <p>Просто нажмите на кнопку ниже:</p>
                <p><a href='{resetLink}' style='display: inline-block; padding: 12px 24px; background: #2e7d32; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Восстановить доступ</a></p>
                <p><small>Или скопируйте ссылку: {resetLink}</small></p>
            </div>
            
            <div style='margin-top: 25px; padding: 15px; background: #fff3cd; border-radius: 5px;'>
                <p><strong>⚠️ Важно:</strong></p>
                <ul>
                    <li>Код и ссылка действительны <strong>1 час</strong></li>
                    <li>Если вы не запрашивали восстановление пароля, проигнорируйте это письмо</li>
                    <li>Никому не сообщайте код</li>
                </ul>
            </div>
            
            <br>
            <p>С уважением,<br>Команда О! Прокат</p>";

        return await SendEmailInternalAsync(user.Email, subject, body, _emailOptions, true);
    }

    public async Task<bool> SendManagerNotificationAsync(string subject, string message)
    {
        if (string.IsNullOrEmpty(_managerOptions.Email))
        {
            _logger.LogWarning("Manager email is not configured");
            return false;
        }

        var fullMessage = $@"
            <h2>{subject}</h2>
            <div style='padding: 15px; background: #f8f9fa; border-radius: 5px; margin: 15px 0;'>
                {message}
            </div>
            <p><strong>Время:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
            <hr>
            <p>Это автоматическое уведомление от системы О! Прокат.</p>";

        return await SendEmailInternalAsync(_managerOptions.Email, $"Уведомление: {subject}", fullMessage, _emailOptions, true);
    }

    public async Task<bool> SendBookingConfirmationAsync(Booking booking, User user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("User {UserId} has no email for booking confirmation", user.Id);
            return false;
        }

        string contractBlock = "";
        if (!string.IsNullOrEmpty(booking.ContractUrl))
        {
            // Нормализуем base URL и путь
            var baseUrl = _baseUrl.TrimEnd('/');
            var contractPath = booking.ContractUrl.StartsWith('/') 
                ? booking.ContractUrl 
                : "/" + booking.ContractUrl;
            var contractFullUrl = baseUrl + contractPath;

            contractBlock = $@"
                <div style='margin-top: 20px; padding: 15px; background: #e3f2fd; border-radius: 8px;'>
                    <p><strong>📄 Договор аренды:</strong></p>
                    <p><a href='{contractFullUrl}' style='display: inline-block; padding: 10px 20px; background: #1976d2; color: white; text-decoration: none; border-radius: 5px;'>Скачать договор</a></p>
                    <p style='margin-top: 10px;'><small>Или скопируйте ссылку: {contractFullUrl}</small></p>
                </div>";
        }

        var subject = "✅ Ваше бронирование подтверждено – О! Прокат";
        var body = $@"
            <h2>Ваше бронирование подтверждено!</h2>
            <div style='background: #e8f5e9; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                <p><strong>Номер бронирования:</strong> {booking.Id}</p>
                <p><strong>Автомобиль:</strong> {booking.Car?.Brand} {booking.Car?.Model} ({booking.Car?.Year})</p>
                <p><strong>Период аренды:</strong> {booking.StartDate:dd.MM.yyyy} – {booking.EndDate:dd.MM.yyyy}</p>
                <p><strong>Количество дней:</strong> {(booking.EndDate - booking.StartDate).Days}</p>
                <p><strong>Стоимость:</strong> {booking.TotalPrice:N0} ₽</p>
                <p><strong>Статус:</strong> Подтверждено ✅</p>
            </div>
            {contractBlock}
            <h3>📋 Инструкции:</h3>
            <ol>
                <li>Приезжайте в офис для получения автомобиля, если иное не было обговорено.</li>
                <li>Возьмите с собой паспорт и водительское удостоверение</li>
                <li>Приходите за 15 минут до начала аренды</li>
            </ol>
            <p><strong>📍 Адрес офиса:</strong> Московская область, Одинцово, микрорайон Новая Трёхгорка, Кутузовская улица, 12</p>
            <p><strong>📞 Телефон:</strong> +7 (968) 287-03-83</p>
            <br>
            <p>С уважением,<br>Команда О! Прокат</p>";

        // Отправляем через EmailOptions или BookingEmailOptions? Лучше через обычные emailOptions
        return await SendEmailInternalAsync(user.Email, subject, body, _emailOptions, true);
    }

    public async Task<bool> SendBookingCancellationAsync(Booking booking, User user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("User {UserId} has no email for booking cancellation", user.Id);
            return false;
        }

        var subject = "Ваше бронирование отменено – О! Прокат";
        var body = $@"
            <h2>Ваше бронирование отменено</h2>
            <div style='background: #ffebee; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                <p><strong>Номер бронирования:</strong> {booking.Id}</p>
                <p><strong>Автомобиль:</strong> {booking.Car?.Brand} {booking.Car?.Model}</p>
                <p><strong>Сумма возврата:</strong> {booking.TotalPrice} ₽</p>
                <p><strong>Дата отмены:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
            </div>
            <p>Если отмена произошла по ошибке или у вас есть вопросы, свяжитесь с поддержкой:</p>
            <p><strong>📧 Email:</strong> support@o-prokat.ru</p>
            <p><strong>📞 Телефон:</strong> +7 (968) 287-03-83</p>
            <p><strong>🌐 Сайт:</strong> <a href='{_baseUrl}'>{_baseUrl}</a></p>
            <br>
            <p>С уважением,<br>Команда О! Прокат</p>";

        return await SendEmailInternalAsync(user.Email, subject, body, _emailOptions, true);
    }

    public async Task<bool> SendNewUserNotificationAsync(User user)
    {
        if (string.IsNullOrEmpty(_managerOptions.Email))
        {
            _logger.LogWarning("Manager email is not configured for new user notification");
            return false;
        }

        var subject = "👤 Новый пользователь зарегистрирован";
        var message = $@"
            <h3>Детали регистрации:</h3>
            <p><strong>Имя:</strong> {user.FirstName} {user.LastName}</p>
            <p><strong>Email:</strong> {user.Email ?? "Не указан"}</p>
            <p><strong>Телефон:</strong> {user.PhoneNumber}</p>
            <p><strong>Дата рождения:</strong> {user.BirthDate:dd.MM.yyyy}</p>
            <p><strong>Возраст:</strong> {DateTime.Now.Year - user.BirthDate.Year} лет</p>
            <p><strong>Стаж вождения:</strong> {user.DrivingExperience} лет</p>
            <p><strong>Дата регистрации:</strong> {user.RegistrationDate:dd.MM.yyyy HH:mm}</p>
            <p><strong>Статус:</strong> {user.Status}</p>
            <p><strong>Email подтвержден:</strong> {(user.EmailConfirmed ? "✅ Да" : "❌ Нет")}</p>";

        return await SendEmailInternalAsync(_managerOptions.Email, subject, message, _emailOptions, true);
    }
    
    public async Task<bool> SendContractToClientAsync(BookingDto booking, string contractUrl)
    {
        if (booking?.User == null || string.IsNullOrEmpty(booking.User.Email))
            return false;

        var subject = $"Договор аренды №{booking.ContractNumber}";
        var body = $@"
            <p>Уважаемый(ая) {booking.User.FirstName},</p>
            <p>Ваше бронирование автомобиля {booking.Car?.Brand} {booking.Car?.Model} подтверждено.</p>
            <p>Договор аренды доступен по ссылке: <a href='{contractUrl}'>Скачать договор</a></p>
            <p>С уважением, команда О! Прокат</p>";
        return await SendEmailAsync(booking.User.Email, subject, body);
    }
}