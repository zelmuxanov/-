using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.User;
using CarRental.Domain.Enums;
using CarRental.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRental.BLL.Services;

namespace CarRental.Web.Areas.Admin.Controllers;

[Route("Admin/Users")]
public class UsersController : BaseAdminController // Наследуемся от BaseAdminController
{
    private readonly IUserService _userService;
    private readonly IDocumentService _documentService;
    private readonly IChatService _chatService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IDocumentService documentService,
        IChatService chatService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _documentService = documentService;
        _chatService = chatService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1, 
        int pageSize = 10, 
        UserStatus? status = null,
        string? search = null)
    {
        try
        {
            ViewData["Title"] = "Управление пользователями";
            
            var (users, totalCount) = await _userService.GetUsersWithPaginationAsync(page, pageSize, status);
            
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                users = users.Where(u => 
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    u.PhoneNumber.ToLower().Contains(searchLower)
                );
                totalCount = users.Count();
            }
            
            var model = new AdminUsersViewModel
            {
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                StatusFilter = status,
                Search = search
            };
            
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(s => new { Value = (int)s, Text = s.ToString() }),
                "Value", "Text", status);
            
            return View(model); // Автоматически использует правильный путь
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке пользователей");
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке пользователей";
            return View(new AdminUsersViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            ViewData["Title"] = "Детали пользователя";
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction(nameof(Index));
            }

            // Получаем документы через IDocumentService
            var documents = await _documentService.GetUserDocumentsAsync(id);
            
            var model = new AdminUserDetailsViewModel
            {
                User = user,
                Documents = documents
            };

            var chat = await _chatService.GetChatByUserIdAsync(id);
            ViewBag.UserChatId = chat?.Id;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке деталей пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке деталей пользователя";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Activate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            _logger.LogInformation("Admin activating user: {UserId}", id);
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                _logger.LogWarning("User not found: {UserId}", id);
            }
            else
            {
                _logger.LogInformation("User info - Email: {Email}, Status: {Status}", 
                    user.Email, user.Status);
                
                var result = await _userService.ActivateUserAsync(id);
                
                if (result)
                {
                    TempData["SuccessMessage"] = $"Пользователь {user.Email} успешно активирован";
                    _logger.LogInformation("User activated: {Email}", user.Email);
                }
                else
                {
                    TempData["ErrorMessage"] = $"Не удалось активировать пользователя {user.Email}. " +
                                            $"Текущий статус: {user.Status}. " +
                                            $"Требуется статус: Pending";
                    _logger.LogWarning("Failed to activate user {Email}. Status: {Status}", 
                        user.Email, user.Status);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при активации пользователя {UserId}", id);
            TempData["ErrorMessage"] = $"Произошла ошибка при активации пользователя: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Reject/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        try
        {
            var result = await _userService.UpdateUserStatusAsync(id, UserStatus.Rejected);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Пользователь отклонен";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось отклонить пользователя";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отклонении пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Произошла ошибка при отклонении пользователя";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Block/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(Guid id)
    {
        try
        {
            var result = await _userService.BlockUserAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Пользователь успешно заблокирован";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось заблокировать пользователя";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при блокировке пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Произошла ошибка при блокировке пользователя";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Unblock/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(Guid id)
    {
        try
        {
            var result = await _userService.UnblockUserAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Пользователь успешно разблокирован";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось разблокировать пользователя";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при разблокировке пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Произошла ошибка при разблокировке пользователя";
        }
        
        return RedirectToAction(nameof(Index));
    }
    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Пользователь успешно удален";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось удалить пользователя";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Произошла ошибка при удалении пользователя";
        }
        
        return RedirectToAction(nameof(Index));
    }
}