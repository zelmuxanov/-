using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using CarRental.Web.ViewModels.Account;
using CarRental.BLL.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CarRental.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserService userService,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _emailService = emailService;
        _logger = logger;
    }

    #region Регистрация
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        try
        {
            Console.WriteLine("=== 🔍 REGISTER DIAGNOSTICS START ===");
            Console.WriteLine($"Email: {model.Email}");
            Console.WriteLine($"FirstName: {model.FirstName}");
            Console.WriteLine($"LastName: {model.LastName}");
            Console.WriteLine($"BirthDate: {model.BirthDate}");
            Console.WriteLine($"DrivingExperience: {model.DrivingExperience}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState invalid");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($" - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            // ✅ ЯВНАЯ ВАЛИДАЦИЯ КРИТИЧЕСКИХ ПОЛЕЙ
            if (!model.BirthDate.HasValue)
            {
                ModelState.AddModelError("BirthDate", "Дата рождения обязательна");
                return View(model);
            }

            if (!model.DrivingExperience.HasValue)
            {
                ModelState.AddModelError("DrivingExperience", "Стаж вождения обязателен");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Пароль обязателен");
                return View(model);
            }

            // ✅ СОЗДАЕМ ПОЛЬЗОВАТЕЛЯ
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                PhoneNumber = model.PhoneNumber,
                BirthDate = new DateTime(model.BirthDate.Value.Year, model.BirthDate.Value.Month, model.BirthDate.Value.Day, 0, 0, 0, DateTimeKind.Utc),
                DrivingExperience = model.DrivingExperience.Value,
                Status = UserStatus.Pending,
                RegistrationDate = DateTime.UtcNow,
                EmailConfirmed = false
            };

            Console.WriteLine("📝 Creating user with UserManager.CreateAsync...");
            
            // ✅ СОЗДАЕМ ПОЛЬЗОВАТЕЛЯ С ПАРОЛЕМ
            var result = await _userManager.CreateAsync(user, model.Password);
            
            Console.WriteLine($"📊 UserManager.CreateAsync result: Succeeded={result.Succeeded}");

            if (result.Succeeded)
            {
                Console.WriteLine("✅ UserManager: пользователь успешно создан");

                var savedUser = await _userManager.FindByEmailAsync(model.Email);
                Console.WriteLine($"🔍 User after creation: {savedUser != null}");

                if (savedUser != null)
                {
                    Console.WriteLine($"📋 User details: ID={savedUser.Id}, Email={savedUser.Email}");

                    // ✅ НАЗНАЧАЕМ РОЛЬ USER
                    await _userManager.AddToRoleAsync(savedUser, "User");
                    
                    // ✅ ПЕРЕДАЕМ ДАЛЬНЕЙШУЮ ОБРАБОТКУ В EmailConfirmationController
                    TempData["NewUserId"] = savedUser.Id.ToString();
                    TempData["RegistrationEmail"] = savedUser.Email;
                    return RedirectToAction("ProcessNewRegistration", "EmailConfirmation");
                }
                else
                {
                    Console.WriteLine("❌ USER NOT FOUND AFTER CREATION!");
                    ModelState.AddModelError(string.Empty, "Пользователь не был сохранен в базу данных");
                }
            }
            else
            {
                Console.WriteLine("❌ USER MANAGER ERRORS:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($" - {error.Code}: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 REGISTRATION EXCEPTION: {ex}");
            _logger.LogError(ex, "Ошибка при регистрации пользователя {Email}", model.Email);
            ModelState.AddModelError(string.Empty, $"Ошибка регистрации: {ex.Message}");
            return View(model);
        }
    }

    #endregion

    #region Вход 
    
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;

            Console.WriteLine("=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Email: {model.Email}");
            Console.WriteLine($"Password: [HIDDEN]");
            Console.WriteLine($"Password length: {model.Password?.Length ?? 0}");
            Console.WriteLine($"RememberMe: {model.RememberMe}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid");
                return View(model);
            }

            // ✅ ЯВНАЯ ВАЛИДАЦИЯ ПАРОЛЯ
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Пароль обязателен");
                return View(model);
            }

            // ✅ ПОИСК ПОЛЬЗОВАТЕЛЯ
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine($"❌ USER NOT FOUND: {model.Email}");
                ModelState.AddModelError(string.Empty, "Пользователь с таким email не найден");
                return View(model);
            }

            Console.WriteLine($"✅ USER FOUND: ID={user.Id}, UserName={user.UserName}");
            Console.WriteLine($"📊 User Status: {user.Status}, EmailConfirmed: {user.EmailConfirmed}");

            // ✅ ИСПРАВЛЕНИЕ: Проверяем только блокировку, не статус Active
            if (user.Status == UserStatus.Blocked)
            {
                Console.WriteLine($"⚠️ USER BLOCKED: Status={user.Status}");
                ModelState.AddModelError(string.Empty, 
                    "Ваш аккаунт заблокирован. Обратитесь к администратору.");
                return View(model);
            }

            // ✅ ПРОВЕРКА ПОДТВЕРЖДЕНИЯ EMAIL
            if (!user.EmailConfirmed)
            {
                Console.WriteLine($"⚠️ EMAIL NOT CONFIRMED for {model.Email}");
                ModelState.AddModelError(string.Empty, 
                    "Email не подтвержден. Проверьте вашу почту или запросите новое письмо.");
                
                // Показываем ссылку для повторной отправки
                TempData["ShowResendLink"] = true;
                TempData["UserEmail"] = user.Email;
                
                return View(model);
            }

            // ✅ ПРОВЕРКА ПАРОЛЯ
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            Console.WriteLine($"🔐 PASSWORD VALID: {passwordValid}");

            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Неверный пароль");
                return View(model);
            }

            // ✅ ВХОД В СИСТЕМУ
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            Console.WriteLine($"Login result: Succeeded={result.Succeeded}");

            if (result.Succeeded)
            {
                Console.WriteLine($"✅ LOGIN SUCCESS for {model.Email}");
                Console.WriteLine($"📊 User Status after login: {user.Status}");
                
                _logger.LogInformation("Пользователь {Email} (Статус: {Status}) вошел в систему", 
                    model.Email, user.Status);
                    
                return RedirectToLocal(returnUrl);
            }
            else
            {
                Console.WriteLine($"❌ LOGIN FAILED for {model.Email}");
                ModelState.AddModelError(string.Empty, "Ошибка входа");
            }

            return View(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 LOGIN ERROR: {ex.Message}");
            _logger.LogError(ex, "Ошибка при входе пользователя {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при входе в систему");
            return View(model);
        }
    }

    #endregion

    #region Выход
    
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            Console.WriteLine("🔄 GET LOGOUT CALLED - User: " + User?.Identity?.Name);
            
            await _signInManager.SignOutAsync();
            
            Console.WriteLine("✅ GET LOGOUT COMPLETED - User signed out");
            
            // ✅ ДОБАВЛЯЕМ ЗАЩИТНЫЕ ЗАГОЛОВКИ
            Response.Headers["Cache-Control"] = "no-cache, no-store";
            Response.Headers["Pragma"] = "no-cache";
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 GET LOGOUT ERROR: {ex.Message}");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Пользователь вышел из системы");
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Вспомогательные методы
    
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        else
            return RedirectToAction("Index", "Home");
    }

    #endregion
}