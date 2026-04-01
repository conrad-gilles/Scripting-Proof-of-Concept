using System;
using System.Collections.Generic;
using BlazorUI.Settings;

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
            if (AppSettings.UseConsole)
            {
                Console.WriteLine(message);
                Logs.Add((DateTime.Now, message));
                if (message.Contains("Error"))
                {
                    IsExpanded = true;
                }
                NotifyStateChanged();
            }
        }
        public void LogException(Exception exception)
        {
            if (AppSettings.UseConsole)
            {
                if (AppSettings.PrintDetailedInConsole)
                {
                    Log(exception.ToString());
                }
                else
                {
                    Log(exception.Message);
                }
            }
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
