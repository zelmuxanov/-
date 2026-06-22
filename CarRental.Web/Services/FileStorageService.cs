using CarRental.BLL.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarRental.Web.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IFileEncryptionService? _encryptionService;
    private readonly ILogger<FileStorageService>? _logger;

    public FileStorageService(
        IWebHostEnvironment environment,
        IFileEncryptionService? encryptionService = null,
        ILogger<FileStorageService>? logger = null)
    {
        _environment = environment;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        // Для чата не используем шифрование
        if (folder == "chat" || folder == "pages" || folder == "cars" || folder == "cars/temp" || folder == "contracts")

        {
            return await SaveFileWithoutEncryptionAsync(file, folder);
        }
        return await SaveSecureFileAsync(file, folder, Guid.Empty);
    }
    private async Task<string> SaveFileWithoutEncryptionAsync(IFormFile file, string folder)
    {
        // простая реализация без шифрования
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsFolder);
        var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, safeFileName);
        using var stream = file.OpenReadStream();
        using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);
        return $"/uploads/{folder}/{safeFileName}";
    }
    public async Task<string> SaveSecureFileAsync(IFormFile file, string folder, Guid userId)
    {
        await using var stream = file.OpenReadStream();
        if (!IsValidFileSignature(stream, file.FileName))
            throw new InvalidOperationException("Файл повреждён или имеет недопустимый формат.");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = $"{Guid.NewGuid()}_{userId}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, safeFileName);
        var relativePath = $"/uploads/{folder}/{safeFileName}";

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            if (_encryptionService != null)
            {
                await _encryptionService.EncryptStreamAsync(stream, fileStream);
                _logger?.LogInformation("Файл {FilePath} сохранён с шифрованием", relativePath);
            }
            else
            {
                await stream.CopyToAsync(fileStream);
                _logger?.LogInformation("Файл {FilePath} сохранён без шифрования", relativePath);
            }
        }

        return relativePath;
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Файл не найден");

        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        using var ms = new MemoryStream();
        if (_encryptionService != null)
        {
            await _encryptionService.DecryptStreamAsync(fileStream, ms);
        }
        else
        {
            await fileStream.CopyToAsync(ms);
        }
        return ms.ToArray();
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger?.LogInformation("Файл {FilePath} удалён", filePath);
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        DeleteFile(filePath);
        return await Task.FromResult(true);
    }

    private bool IsValidFileSignature(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var signatures = extension switch
        {
            ".jpg" or ".jpeg" => new byte[][] { new byte[] { 0xFF, 0xD8, 0xFF } },
            ".png" => new byte[][] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            ".pdf" => new byte[][] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            _ => Array.Empty<byte[]>()
        };

        if (signatures.Length == 0) return false;

        foreach (var magic in signatures)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var header = new byte[magic.Length];
            var bytesRead = stream.Read(header, 0, magic.Length);
            if (bytesRead == magic.Length && header.SequenceEqual(magic))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
        }
        return false;
    }
    public async Task<string> SaveEncryptedFileAsync(byte[] data, string fileName, string folder)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsFolder);
        var safeFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadsFolder, safeFileName);
        var relativePath = $"/uploads/{folder}/{safeFileName}";

        using var stream = new MemoryStream(data);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        if (_encryptionService != null)
        {
            await _encryptionService.EncryptStreamAsync(stream, fileStream);
            _logger?.LogInformation("Файл {FilePath} сохранён с шифрованием", relativePath);
        }
        else
        {
            await stream.CopyToAsync(fileStream);
        }
        return relativePath;
    }
}