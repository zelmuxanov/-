using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Создаем роли, если не существуют
        var roles = new[] { "Admin", "User" };
        
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                Console.WriteLine($"✅ Роль '{roleName}' создана");
            }
        }

        // Создаем начального администратора, если нет
        var adminEmail = "admin@carrental.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Администратор",
                LastName = "Системы",
                BirthDate = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DrivingExperience = 20,
                Status = UserStatus.Active,
                RegistrationDate = DateTime.UtcNow,
                EmailConfirmed = true,
                PhoneNumber = "+79991234567"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"✅ Администратор создан: {adminEmail} / Admin123!");
            }
            else
            {
                Console.WriteLine($"❌ Ошибка создания администратора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Проверяем, есть ли у администратора роль
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"✅ Роль Admin добавлена существующему пользователю {adminEmail}");
            }
        }
    }
}