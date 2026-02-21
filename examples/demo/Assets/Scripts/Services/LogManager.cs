using System;
using System.Collections.Generic;

namespace OneSignalDemo.Services
{
    public enum LogEntryLevel { Debug, Info, Warn, Error }

    public class LogEntry
    {
        public DateTime Timestamp { get; }
        public LogEntryLevel Level { get; }
        public string Tag { get; }
        public string Message { get; }

        public LogEntry(LogEntryLevel level, string tag, string message)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Tag = tag;
            Message = message;
        }

        public string LevelChar => Level switch
        {
            LogEntryLevel.Debug => "D",
            LogEntryLevel.Info => "I",
            LogEntryLevel.Warn => "W",
            LogEntryLevel.Error => "E",
            _ => "?"
        };
    }

    public class LogManager
    {
        private static readonly LogManager _instance = new();
        public static LogManager Instance => _instance;

        private readonly List<LogEntry> _entries = new();
        public IReadOnlyList<LogEntry> Entries => _entries;

        public event Action<LogEntry> OnLogAdded;

        private LogManager() { }

        public void Debug(string tag, string message) => Add(LogEntryLevel.Debug, tag, message);
        public void Info(string tag, string message) => Add(LogEntryLevel.Info, tag, message);
        public void Warn(string tag, string message) => Add(LogEntryLevel.Warn, tag, message);
        public void Error(string tag, string message) => Add(LogEntryLevel.Error, tag, message);

        public void Clear()
        {
            _entries.Clear();
            OnLogAdded?.Invoke(null);
        }

        private void Add(LogEntryLevel level, string tag, string message)
        {
            var entry = new LogEntry(level, tag, message);
            _entries.Add(entry);

            switch (level)
            {
                case LogEntryLevel.Debug:
                    UnityEngine.Debug.Log($"[{tag}] {message}");
                    break;
                case LogEntryLevel.Info:
                    UnityEngine.Debug.Log($"[{tag}] {message}");
                    break;
                case LogEntryLevel.Warn:
                    UnityEngine.Debug.LogWarning($"[{tag}] {message}");
                    break;
                case LogEntryLevel.Error:
                    UnityEngine.Debug.LogError($"[{tag}] {message}");
                    break;
            }

            OnLogAdded?.Invoke(entry);
        }
    }
}
