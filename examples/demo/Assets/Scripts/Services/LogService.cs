using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneSignalDemo.Services
{
    public readonly struct LogEntry
    {
        public readonly string Time;
        public readonly string Message;

        public LogEntry(string time, string message)
        {
            Time = time;
            Message = message;
        }
    }

    public static class LogService
    {
        private const int MaxEntries = 100;

        // In memory only, so each launch starts empty and the Appium suite can assert per-launch counts.
        private static readonly List<LogEntry> _entries = new();

        public static IReadOnlyList<LogEntry> Entries => _entries;

        public static event Action Changed;

        public static void Info(string message) => Debug.Log($"[OneSignal] {message}");

        public static void Show(string message)
        {
            Info(message);
            AddToView(message);
        }

        public static void Error(string message)
        {
            Debug.LogError($"[OneSignal] {message}");
            AddToView(message);
        }

        public static void Clear()
        {
            _entries.Clear();
            Changed?.Invoke();
        }

        private static void AddToView(string message)
        {
            _entries.Add(new LogEntry(DateTime.Now.ToString("HH:mm:ss"), message));
            if (_entries.Count > MaxEntries)
                _entries.RemoveRange(0, _entries.Count - MaxEntries);
            Changed?.Invoke();
        }
    }
}
