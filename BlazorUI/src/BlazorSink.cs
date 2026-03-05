using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
namespace BlazorUI.Sink;
public class BlazorSink : ILogEventSink
{
    // Store the last 500 logs to prevent memory leaks
    private readonly int _maxLogs = 500;
    
    // ConcurrentQueue is thread-safe, important for Serilog
    public ConcurrentQueue<LogEvent> LogEvents { get; } = new();

    // Event to notify Blazor UI when a new log comes in
    public event Action? OnLogReceived;

    public void Emit(LogEvent logEvent)
    {
        LogEvents.Enqueue(logEvent);

        // Keep the queue size under control
        while (LogEvents.Count > _maxLogs)
        {
            LogEvents.TryDequeue(out _);
        }

        // Trigger UI update
        OnLogReceived?.Invoke();
    }
}
