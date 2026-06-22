using CarRental.Domain.Enums;
namespace CarRental.BLL.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int DrivingExperience { get; set; }
    public UserStatus Status { get; set; }
    public DateTime RegistrationDate { get; set; }
}
