using Ember.Scripting;

// Interfaces
public interface IConsoleLoggerInterface
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}

public interface IDataAccessInterface
{
    // Simulates fetching external data (e.g., "Is this doctor valid?")
    object? GetReferenceData(string category, string key);
}

// Implementations
public class ConsoleLogger : IConsoleLoggerInterface
{
    public void Info(string message) => Console.WriteLine($"[INFO] {message}");
    public void Warning(string message) => Console.WriteLine($"[WARN] {message}");
    public void Error(string message) => Console.WriteLine($"[ERROR] {message}");
}

public class DataAccess : IDataAccessInterface
{
    public object? GetReferenceData(string category, string key)
    {
        // Mock data for PoC
        if (category == "Doctor" && key == "Dr.House") return new { Name = "Gregory House", Specialty = "Diagnostic" };
        return null;
    }
}
