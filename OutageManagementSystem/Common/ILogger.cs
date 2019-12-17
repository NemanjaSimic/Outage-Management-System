using System;

namespace Outage.Common
{
    public interface ILogger
    {
        void LogInfo(string message, Exception e = null);

        void LogDebug(string message, Exception e = null);

        void LogWarn(string message, Exception e = null);

        void LogError(string message, Exception e = null);

        void LogFatal(string message, Exception e = null);
    }
}
