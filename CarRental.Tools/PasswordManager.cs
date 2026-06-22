using System.Security.Cryptography;
using System.Text;

namespace CarRental.Tools;

public class PasswordManager
{
    public static void Main()
    {
        Console.WriteLine("🔐 Менеджер паролей для Car Rental");
        Console.WriteLine("==================================");
        
        while (true)
        {
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1. Зашифровать пароль SMTP");
            Console.WriteLine("2. Зашифровать токен Telegram");
            Console.WriteLine("3. Проверить шифрование");
            Console.WriteLine("4. Выход");
            
            Console.Write("\nВаш выбор: ");
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    EncryptSmtpPassword();
                    break;
                case "2":
                    EncryptTelegramToken();
                    break;
                case "3":
                    TestEncryption();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }
        }
    }
    
    private static void EncryptSmtpPassword()
    {
        Console.Write("Введите SMTP пароль: ");
        var password = Console.ReadLine();
        
        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Пароль не может быть пустым");
            return;
        }
        
        var encrypted = Encrypt(password);
        Console.WriteLine($"\n🔐 Зашифрованный пароль:");
        Console.WriteLine($"enc:{encrypted}");
        Console.WriteLine($"\n📋 Для appsettings.json:");
        Console.WriteLine($"\"Password\": \"enc:{encrypted}\"");
        
        // Проверка дешифрования
        var decrypted = Decrypt(encrypted);
        Console.WriteLine($"\n✅ Проверка: {decrypted == password}");
    }
    
    private static void EncryptTelegramToken()
    {
        Console.Write("Введите Telegram токен: ");
        var token = Console.ReadLine();
        
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Токен не может быть пустым");
            return;
        }
        
        var encrypted = Encrypt(token);
        Console.WriteLine($"\n🔐 Зашифрованный токен:");
        Console.WriteLine($"enc:{encrypted}");
        Console.WriteLine($"\n📋 Для appsettings.json:");
        Console.WriteLine($"\"BotToken\": \"enc:{encrypted}\"");
    }
    
    private static void TestEncryption()
    {
        var testData = new[]
        {
            "_Zelmuxanov25",
            "8583568342:AAFUPGZipv1inOtv-C7FljiyllomvDhX1Mg",
            "TestPassword123!"
        };
        
        Console.WriteLine("\n🔍 Тест шифрования:");
        Console.WriteLine("====================");
        
        foreach (var data in testData)
        {
            var encrypted = Encrypt(data);
            var decrypted = Decrypt(encrypted);
            
            Console.WriteLine($"\nОригинал: {data}");
            Console.WriteLine($"Зашифрованный: enc:{encrypted}");
            Console.WriteLine($"Расшифрованный: {decrypted}");
            Console.WriteLine($"✅ Совпадение: {data == decrypted}");
        }
    }
    
    private static string Encrypt(string plainText)
    {
        var key = "YourSuperSecretKey32Chars!!1234567890";
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        
        return Convert.ToBase64String(ms.ToArray());
    }
    
    private static string Decrypt(string cipherText)
    {
        try
        {
            var key = "YourSuperSecretKey32Chars!!1234567890";
            var buffer = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];
            
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }
        catch
        {
            return "Ошибка дешифрования";
        }
    }
}