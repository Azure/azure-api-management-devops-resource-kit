using Microsoft.Extensions.Logging;
using System;

namespace apimtemplate.Common
{
    /// <summary>
    /// Helper class for logging any actions. 
    /// You can setup logger passing Microsoft.Extensions.Logging.ILogger that works the best for you.
    /// </summary>
    public static class Logger
    {
        private static ILogger _logger;

        /// <summary>
        /// Specify which logger to use for internal apimtemplate library messaging
        /// </summary>
        /// <param name="logger">end-user logger</param>
        public static void SetupLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            _logger.LogWarning(exception, message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public static void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }
    }
}
