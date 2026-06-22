using Microsoft.AspNetCore.Http;

namespace CarRental.BLL.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    Task<string> SaveSecureFileAsync(IFormFile file, string folder, Guid userId);
    Task<byte[]> ReadFileAsync(string filePath);
    void DeleteFile(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> SaveEncryptedFileAsync(byte[] data, string fileName, string folder);
}