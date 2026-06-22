using System.Security.Cryptography;
using System.Text;

namespace CarRental.BLL.Utilities;

public static class PasswordEncryptor
{
    private const string EncryptionKey = "YourSuperSecretKey32Chars!!1234567890";
    
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;
            
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        
        return "enc:" + Convert.ToBase64String(ms.ToArray());
    }
    
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText) || !cipherText.StartsWith("enc:"))
            return cipherText;
            
        try
        {
            var encrypted = cipherText.Substring(4);
            var buffer = Convert.FromBase64String(encrypted);
            
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];
            
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }
        catch
        {
            return cipherText;
        }
    }
    
    // Утилита для консоли разработчика
    public static void GenerateEncryptedPasswords()
    {
        Console.WriteLine("=== Генерация зашифрованных паролей ===");
        
        var smtpPassword = "_Zelmuxanov25";
        var encryptedSmtp = Encrypt(smtpPassword);
        
        var botToken = "8583568342:AAFUPGZipv1inOtv-C7FljiyllomvDhX1Mg";
        var encryptedBot = Encrypt(botToken);
        
        Console.WriteLine("\n📧 SMTP Password:");
        Console.WriteLine($"Оригинал: {smtpPassword}");
        Console.WriteLine($"Зашифрованный: {encryptedSmtp}");
        
        Console.WriteLine("\n🤖 Telegram Bot Token:");
        Console.WriteLine($"Оригинал: {botToken}");
        Console.WriteLine($"Зашифрованный: {encryptedBot}");
        
        Console.WriteLine("\n📋 Для appsettings.json:");
        Console.WriteLine($"\"Password\": \"{encryptedSmtp}\"");
        Console.WriteLine($"\"BotToken\": \"{encryptedBot}\"");
        
        Console.WriteLine("\n🔐 Ключ шифрования:");
        Console.WriteLine($"Key: {EncryptionKey}");
        Console.WriteLine("\n==================================");
    }
}