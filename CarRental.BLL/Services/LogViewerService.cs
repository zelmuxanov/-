using CarRental.BLL.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CarRental.BLL.Services;

public class LogViewerService : ILogViewerService
{
    private readonly InMemoryLogProvider _provider;

    public LogViewerService(InMemoryLogProvider provider)
    {
        _provider = provider;
    }

    public IReadOnlyCollection<InMemoryLogEntry> GetRecentLogs(int count = 50, LogLevel? minLevel = null)
    {
        var query = _provider.GetEntries().AsEnumerable();
        if (minLevel.HasValue)
            query = query.Where(e => e.Level >= minLevel.Value);
        return query.OrderByDescending(e => e.Timestamp).Take(count).ToList().AsReadOnly();
    }
}