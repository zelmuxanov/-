using CarRental.BLL.Services;
using Microsoft.Extensions.Logging;

namespace CarRental.BLL.Interfaces.Services;

public interface ILogViewerService
{
    IReadOnlyCollection<InMemoryLogEntry> GetRecentLogs(int count = 50, LogLevel? minLevel = null);
}