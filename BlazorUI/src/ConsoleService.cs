using System;
using System.Collections.Generic;

namespace BlazorUI.Services
{
    public class ConsoleService
    {
        public List<(DateTime timestamp, string message)> Logs { get; private set; } = new();
        public bool IsExpanded { get; set; } = false;

        // Event to tell the UI to re-render when a log is added
        public event Action? OnChange;

        public void Log(string message)
        {
            Console.WriteLine(message);
            Logs.Add((DateTime.Now, message));
            if (message.Contains("Error"))
            {
                IsExpanded = true;
            }
            NotifyStateChanged();
        }

        public void Clear()
        {
            Logs.Clear();
            NotifyStateChanged();
        }

        public void Toggle()
        {
            IsExpanded = !IsExpanded;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
