using CarRental.Domain.Entities;

namespace CarRental.BLL.Interfaces.Services;

public interface IContractService
{
    Task<string> GenerateContractPdfAsync(Booking booking);
    Task<string> GenerateContractDocxAsync(Booking booking);
    
}