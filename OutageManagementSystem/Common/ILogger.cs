using System;

namespace Outage.Common
{
    public interface ILogger
    {
        void LogInfo(string message, Exception e);

        void LogDebug(string message, Exception e);

        void LogWarn(string message, Exception e);

        void LogError(string message, Exception e);

        void LogFatal(string message, Exception e);
    }
}
