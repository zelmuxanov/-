using System.Security.Cryptography;

namespace CarRental.Tools;

/// <summary>
/// Утилита для генерации безопасного ключа шифрования AES-256.
/// Запускается один раз при первоначальной настройке сервера.
/// Результат прописывается в переменную окружения FileEncryption__Key.
/// </summary>
public class PasswordManager
{
    public static void Main()
    {
        Console.WriteLine("🔐 Генератор ключа шифрования — CarRental");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("Этот инструмент генерирует случайный AES-256 ключ.");
        Console.WriteLine("Никогда не храните ключ в коде или appsettings.json!");
        Console.WriteLine();

        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var keyBase64 = Convert.ToBase64String(keyBytes);

        Console.WriteLine("✅ Сгенерированный ключ (скопируй в переменную окружения):");
        Console.WriteLine();
        Console.WriteLine(keyBase64);
        Console.WriteLine();
        Console.WriteLine("Команда PowerShell для Windows Server:");
        Console.WriteLine($"[System.Environment]::SetEnvironmentVariable(\"FileEncryption__Key\", \"{keyBase64}\", [System.EnvironmentVariableTarget]::Machine)");
        Console.WriteLine();
        Console.WriteLine("⚠️  Сохрани ключ в безопасном месте (KeePass, BitWarden и т.д.)");
        Console.WriteLine("    При потере ключа зашифрованные документы будут недоступны!");
    }
}