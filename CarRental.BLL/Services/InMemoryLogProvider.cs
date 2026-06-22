using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CarRental.BLL.Services;

public class InMemoryLogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
}

public class InMemoryLogProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<InMemoryLogEntry> _entries = new();
    private const int MaxEntries = 500;

    public IReadOnlyCollection<InMemoryLogEntry> GetEntries() => _entries.ToList().AsReadOnly();

    public void AddEntry(LogLevel level, string category, string message, Exception? exception)
    {
        _entries.Enqueue(new InMemoryLogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Category = category,
            Message = message,
            Exception = exception?.ToString()
        });

        while (_entries.Count > MaxEntries)
            _entries.TryDequeue(out _);
    }

    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(this, categoryName);
    public void Dispose() { }

    private class InMemoryLogger : ILogger
    {
        private readonly InMemoryLogProvider _provider;
        private readonly string _category;

        public InMemoryLogger(InMemoryLogProvider provider, string category)
        {
            _provider = provider;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning; // Warning и выше

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            var message = formatter(state, exception);
            _provider.AddEntry(logLevel, _category, message, exception);
        }
    }
}