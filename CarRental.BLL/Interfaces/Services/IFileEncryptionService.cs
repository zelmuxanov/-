namespace CarRental.BLL.Interfaces.Services;

public interface IFileEncryptionService
{
    Task EncryptStreamAsync(Stream input, Stream output);
    Task DecryptStreamAsync(Stream input, Stream output);
}