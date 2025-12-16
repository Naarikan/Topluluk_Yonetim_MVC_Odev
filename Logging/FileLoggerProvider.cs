using System.Collections.Concurrent;

namespace Topluluk_Yonetim.MVC.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logFilePath;
        private readonly LogLevel _minLevel;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

        public FileLoggerProvider(string logFilePath, LogLevel minLevel)
        {
            _logFilePath = logFilePath;
            _minLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName,
                name => new FileLogger(name, _logFilePath, _minLevel));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}


