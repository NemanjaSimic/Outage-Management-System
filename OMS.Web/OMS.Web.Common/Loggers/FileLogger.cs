namespace OMS.Web.Common.Loggers
{
    using NLog;
    using System;
    using ILogger = Outage.Common.ILogger;

    public class FileLogger : ILogger
    {
        private static Logger _logger;

        public FileLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void LogDebug(string message, Exception e = null)
        {
            _logger.Debug(message);
        }

        public void LogError(Exception e, string message = null)
        {
            _logger.Error(e, message ?? e.Message);
        }

        public void LogError(string message, Exception e = null)
        {
            _logger.Error(e, message ?? e.Message);
        }

        public void LogFatal(string message, Exception e = null)
        {
            _logger.Fatal(e, message ?? e.Message);
        }

        public void LogInfo(string message, Exception e = null)
        {
            _logger.Info(message);
        }

        public void LogWarn(string message, Exception e = null)
        {
            _logger.Warn(message);
        }
    }
}
