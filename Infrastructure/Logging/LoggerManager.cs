using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace Infrastructure.Logging
{
    public class LoggerManager : ILoggerManager
    {
        private readonly ILogger<LoggerManager> _logger;

        public LoggerManager(ILogger<LoggerManager> logger)
        {
            _logger = logger;
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogError(Exception ex, string message = "")
        {
            if (string.IsNullOrEmpty(message))
                _logger.LogError(ex, ex.Message);
            else
                _logger.LogError(ex, message);
        }

        public void LogInfo(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarn(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }
    }
} 