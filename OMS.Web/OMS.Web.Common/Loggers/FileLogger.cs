using NLog;
using System;
using ILogger = OMS.Web.Common.Interfaces.Logger.ILogger;

namespace OMS.Web.Common.Loggers
{
    public class FileLogger : ILogger
    {
        private static Logger _logger;

        public FileLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public void LogError(Exception e, string message = null)
        {
            _logger.Error(e, message ?? e.Message);
        }
    }
}
