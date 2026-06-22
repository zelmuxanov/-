namespace CarRental.Domain.Enums;

public enum UserStatus
{
    Pending = 1,      // На рассмотрении
    Active = 2,       // Активен (должно быть)
    Blocked = 3,      // Заблокирован
    Rejected = 4      // Отклонен
}