// Services/DebugLogger.cs
using System;
using System.IO;

namespace XlsxMerger.Services
{
    public enum LogLevel
    {
        INFO,
        WARN,
        ERROR,
        DEBUG
    }

    /// <summary>
    /// Simple logging service that writes messages to a debug.log file.
    /// </summary>
    public class DebugLogger
    {
        private string _logPath;
        private object _lockObj = new object(); // Ensures thread safety

        public DebugLogger()
        {
            // The log file will be created next to the .exe file
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _logPath = Path.Combine(baseDir, "debug.log");
        }

        public void Log(LogLevel level, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string line = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";

                // Use lock to prevent errors if multiple threads write at the same time
                lock (_lockObj)
                {
                    File.AppendAllText(_logPath, line);
                }
            }
            catch
            {
                // Silently ignore logging errors (e.g. disk full) to not crash the main app
            }
        }
    }
}