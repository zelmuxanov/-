using CarRental.BLL.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace CarRental.BLL.Services;

public class AesGcmFileEncryptionService : IFileEncryptionService
{
    private readonly byte[] _key;

    public AesGcmFileEncryptionService(IConfiguration configuration)
    {
        var base64Key = configuration["FileEncryption:Key"];
        if (string.IsNullOrEmpty(base64Key))
            throw new InvalidOperationException("FileEncryption:Key is not configured.");
        _key = Convert.FromBase64String(base64Key);
        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 32 bytes for AES-256.");
    }

    public async Task EncryptStreamAsync(Stream input, Stream output)
    {
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);

        await output.WriteAsync(nonce, 0, nonce.Length);

        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        using var ms = new MemoryStream();
        await input.CopyToAsync(ms);
        var plainBytes = ms.ToArray();

        var cipherBytes = new byte[plainBytes.Length];
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        await output.WriteAsync(cipherBytes, 0, cipherBytes.Length);
        await output.WriteAsync(tag, 0, tag.Length);
    }

    public async Task DecryptStreamAsync(Stream input, Stream output)
    {
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        await input.ReadExactlyAsync(nonce, 0, nonce.Length);

        using var ms = new MemoryStream();
        await input.CopyToAsync(ms);
        var fullData = ms.ToArray();

        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        Array.Copy(fullData, fullData.Length - tag.Length, tag, 0, tag.Length);
        var cipherBytes = new byte[fullData.Length - tag.Length];
        Array.Copy(fullData, 0, cipherBytes, 0, cipherBytes.Length);

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        var plainBytes = new byte[cipherBytes.Length];
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        await output.WriteAsync(plainBytes, 0, plainBytes.Length);
    }
}