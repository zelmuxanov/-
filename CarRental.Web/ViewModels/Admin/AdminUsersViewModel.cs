using CarRental.BLL.DTOs.Document;
using CarRental.BLL.DTOs.User;

namespace CarRental.Web.ViewModels.Admin;

public class AdminUsersViewModel
{
    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public CarRental.Domain.Enums.UserStatus? StatusFilter { get; set; }
    public string? Search { get; set; }
}

public class AdminUserDetailsViewModel
{
    public UserDto User { get; set; } = null!;
    public IEnumerable<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
}