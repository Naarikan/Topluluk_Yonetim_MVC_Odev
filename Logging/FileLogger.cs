using Microsoft.Extensions.Logging;

namespace Topluluk_Yonetim.MVC.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logFilePath;
        private readonly LogLevel _minLevel;

        public FileLogger(string categoryName, string logFilePath, LogLevel minLevel)
        {
            _categoryName = categoryName;
            _logFilePath = logFilePath;
            _minLevel = minLevel;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
                return;

            var line =
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} " +
                $"[{logLevel}] {_categoryName} - {message}";

            if (exception != null)
                line += Environment.NewLine + exception;

            line += Environment.NewLine;

            try
            {
                var dir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.AppendAllText(_logFilePath, line);
            }
            catch
            {
            }
        }
    }
}


