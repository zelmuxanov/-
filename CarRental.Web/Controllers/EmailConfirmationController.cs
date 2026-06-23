using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using CarRental.Web.ViewModels.Account;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CarRental.Web.Controllers;

public class EmailConfirmationController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailConfirmationController> _logger;

    public EmailConfirmationController(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<EmailConfirmationController> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ProcessNewRegistration()
    {
        try
        {
            if (TempData["NewUserId"] == null || TempData["RegistrationEmail"] == null)
                return RedirectToAction("Register", "Account");

            var userId = TempData["NewUserId"]?.ToString();
            var email = TempData["RegistrationEmail"]?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "ID пользователя не найден";
                return RedirectToAction("Register", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction("Register", "Account");
            }

            // ✅ ГЕНЕРИРУЕМ ТОКЕН ПОДТВЕРЖДЕНИЯ EMAIL
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
            
            // ✅ ГЕНЕРИРУЕМ 6-ЗНАЧНЫЙ КОД
            var confirmationCode = GenerateConfirmationCode();
            
            // ✅ СОХРАНЯЕМ ТОКЕН И КОД В БАЗУ
            user.EmailConfirmationToken = emailToken;
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.EmailConfirmationCode = confirmationCode;
            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(user);
            
            // EMAIL С ПОДТВЕРЖДЕНИЕМ (ТОКЕН И КОД)
            await _emailService.SendEmailConfirmationAsync(user, encodedToken, confirmationCode);
           
            await _emailService.SendNewUserNotificationAsync(user);

            _logger.LogInformation("Подтверждение по email отправлено пользователю");            
            TempData["RegistrationSuccess"] = true;
            TempData["RegistrationEmail"] = user.Email;
            return RedirectToAction("RegistrationSuccess");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке новой регистрации");
            TempData["ErrorMessage"] = "Ошибка при отправке письма подтверждения";
            return RedirectToAction("Register", "Account");
        }
    }

    [HttpGet]
    public IActionResult RegistrationSuccess()
    {
        if (TempData["RegistrationSuccess"] == null)
            return RedirectToAction("Register", "Account");
            
        ViewBag.Email = TempData["RegistrationEmail"]?.ToString();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var viewModel = new ConfirmEmailViewModel();
        
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                viewModel.IsSuccess = false;
                viewModel.Message = "Неверная ссылка подтверждения";
                return View(viewModel);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                viewModel.IsSuccess = false;
                viewModel.Message = "Пользователь не найден";
                return View(viewModel);
            }

            // Декодируем токен
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpiry = null;
                user.EmailConfirmationCode = null;
                user.EmailConfirmationCodeExpiry = null;
                await _userManager.UpdateAsync(user);
                // Уведомление менеджеру
                try
                {
                    await _emailService.SendManagerNotificationAsync(
                        "✅ Email подтверждён",
                        $"Пользователь {user.FirstName} {user.LastName} ({user.Email}) подтвердил email. " +
                        $"ID: {user.Id}. Статус: {user.Status}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить уведомление менеджеру о подтверждении email");
                }
                
                
                viewModel.IsSuccess = true;
                viewModel.Message = "Email успешно подтвержден! Ваш аккаунт ожидает активации администратором.";
                viewModel.Email = user.Email;
                
                _logger.LogInformation("Пользователь {Email} подтвердил email", user.Email);
            }
            else
            {
                viewModel.IsSuccess = false;
                viewModel.Message = result.Errors.FirstOrDefault()?.Description ?? 
                                  "Ошибка при подтверждении email. Возможно, ссылка устарела.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении email для userId: {UserId}", userId);
            viewModel.IsSuccess = false;
            viewModel.Message = "Произошла ошибка при обработке запроса";
        }
        
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult ConfirmByCode()
    {
        return View(new ConfirmByCodeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmByCode(ConfirmByCodeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный email или код подтверждения");
                return View(model);
            }

            // Проверяем код
            if (user.EmailConfirmationCode != model.Code || 
                !user.EmailConfirmationCodeExpiry.HasValue ||
                user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Неверный или просроченный код подтверждения");
                return View(model);
            }

            // Подтверждаем email
            user.EmailConfirmed = true;
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeExpiry = null;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;
            
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {        
                try
                {
                    await _emailService.SendManagerNotificationAsync(
                        "✅ Email подтверждён",
                        $"Пользователь {user.FirstName} {user.LastName} ({user.Email}) подтвердил email кодом. " +
                        $"ID: {user.Id}. Статус: {user.Status}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить уведомление менеджеру о подтверждении email");
                }        
                TempData["Message"] = "✅ Email успешно подтвержден! Ваш аккаунт ожидает активации администратором.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ошибка при подтверждении email");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении email по коду для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при обработке запроса");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ResendConfirmation()
    {
        return View(new ResendConfirmationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Message"] = "Если пользователь с таким email существует, письмо будет отправлено";
                return View(model);
            }

            if (user.EmailConfirmed)
            {
                TempData["Message"] = "Ваш email уже подтвержден. Вы можете войти в систему.";
                return RedirectToAction("Login", "Account");
            }

            // Проверяем, не отправляли ли мы недавно письмо
            if (user.EmailConfirmationTokenExpiry.HasValue && 
                user.EmailConfirmationTokenExpiry > DateTime.UtcNow)
            {
                var timeLeft = user.EmailConfirmationTokenExpiry.Value - DateTime.UtcNow;
                if (timeLeft.TotalHours > 20)
                {
                    TempData["Message"] = "Письмо с подтверждением уже было отправлено. " +
                                         "Проверьте папку \"Спам\" или попробуйте позже.";
                    return View(model);
                }
            }

            // Генерируем новый токен
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            // Генерируем новый код
            var confirmationCode = GenerateConfirmationCode();
            
            // Обновляем данные пользователя
            user.EmailConfirmationToken = token;
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.EmailConfirmationCode = confirmationCode;
            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(user);
            
            // Отправляем письмо с токеном и кодом
            await _emailService.SendEmailConfirmationAsync(user, encodedToken, confirmationCode);
            
            TempData["Message"] = "Письмо с подтверждением отправлено на ваш email. " +
                                 "Проверьте папку \"Спам\", если не видите письмо.";
            
            _logger.LogInformation("Повторная отправка подтверждения для {Email}", model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при повторной отправке подтверждения для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при отправке письма");
            return View(model);
        }
        
        return View(model);
    }

    private string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}