using System.ComponentModel.DataAnnotations;

namespace CarRental.BLL.DTOs.User;

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; } // ДОЛЖЕН БЫТЬ nullable
    public int? DrivingExperience { get; set; } // ДОЛЖЕН БЫТЬ nullable
    public string Password { get; set; } = string.Empty;
} 