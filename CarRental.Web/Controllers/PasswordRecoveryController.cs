// File: PasswordRecoveryController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using CarRental.Web.ViewModels.Account;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CarRental.Web.Controllers;

public class PasswordRecoveryController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordRecoveryController> _logger;

    public PasswordRecoveryController(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<PasswordRecoveryController> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    // ШАГ 1: Запрос восстановления
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Не раскрываем информацию о существовании пользователя
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Генерируем токен и код
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetCode = GenerateConfirmationCode();
            
            // Сохраняем в базе
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddHours(1);
            
            await _userManager.UpdateAsync(user);
            
            // Отправляем письмо с ССЫЛКОЙ И КОДОМ
            await _emailService.SendPasswordResetAsync(user, encodedToken, resetCode);
            
            // Сохраняем email для следующего шага
            TempData["ResetEmail"] = user.Email;
            
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе сброса пароля для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при запросе сброса пароля");
            return View(model);
        }
    }

    // ШАГ 2: Подтверждение (ввод кода или переход по ссылке)
    [HttpGet] // Явно указываем метод GET
    public IActionResult ForgotPasswordConfirmation()
    {
        var email = TempData["ResetEmail"] as string;
        if (!string.IsNullOrEmpty(email))
        {
            ViewBag.Email = email;
            TempData.Keep("ResetEmail"); // Сохраняем данные для следующего запроса
        }
        return View(new VerifyResetCodeViewModel { Email = email ?? string.Empty });
    }

    // ШАГ 3: Проверка кода
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyResetCode(VerifyResetCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Email = model.Email;
            return View("ForgotPasswordConfirmation", model);
        }

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
                ViewBag.Email = model.Email;
                return View("ForgotPasswordConfirmation", model);
            }

            // Проверяем код
            if (string.IsNullOrEmpty(user.PasswordResetCode) || 
                user.PasswordResetCode != model.Code || 
                !user.PasswordResetCodeExpiry.HasValue ||
                user.PasswordResetCodeExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("Code", "Неверный или просроченный код подтверждения");
                ViewBag.Email = model.Email;
                return View("ForgotPasswordConfirmation", model);
            }

            // Код верный, перенаправляем на сброс пароля
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.PasswordResetToken!));
            
            // Сохраняем в TempData для следующего шага
            TempData["ResetToken"] = encodedToken;
            TempData["ResetEmail"] = user.Email;
            
            return RedirectToAction(nameof(ResetPassword));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке кода для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при проверке кода");
            ViewBag.Email = model.Email;
            return View("ForgotPasswordConfirmation", model);
        }
    }

    // ШАГ 4: Сброс пароля (GET)
    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            string? tempToken = TempData["ResetToken"] as string;
            string? tempEmail = TempData["ResetEmail"] as string;
            
            if (string.IsNullOrEmpty(tempToken) || string.IsNullOrEmpty(tempEmail))
                return RedirectToAction(nameof(ForgotPassword));
            
            token = tempToken;
            email = tempEmail;
        }
        
        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };
        TempData["ResetToken"] = token;
        TempData["ResetEmail"] = email;
        return View(model);
    }

    // ШАГ 4: Сброс пароля (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            // Декодируем токен
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            
            // Сбрасываем пароль
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            
            if (result.Succeeded)
            {
                // Очищаем временные данные
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.PasswordResetCode = null;
                user.PasswordResetCodeExpiry = null;
                await _userManager.UpdateAsync(user);
                
                // Очищаем TempData
                TempData.Remove("ResetToken");
                TempData.Remove("ResetEmail");
                
                _logger.LogInformation("Пользователь {Email} успешно сменил пароль", model.Email);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сбросе пароля для {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при сбросе пароля");
        }
        
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    // Удаляем ненужные методы
    // [HttpGet] public IActionResult ForgotPasswordByCode() - УДАЛЯЕМ
    // [HttpPost] public async Task<IActionResult> ForgotPasswordByCode() - УДАЛЯЕМ
    // [HttpGet] public IActionResult ResetPasswordByCode() - УДАЛЯЕМ
    // [HttpPost] public async Task<IActionResult> ResetPasswordByCode() - УДАЛЯЕМ

    private string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}