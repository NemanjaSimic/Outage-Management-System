using System;

namespace OMS.Common.Cloud.Logger
{
    public interface ICloudLogger
    {
        void LogVerbose(string message, Exception e = null);
        void LogInformation(string message, Exception e = null);
        void LogDebug(string message, Exception e = null);
        void LogWarning(string message, Exception e = null);
        void LogError(string message, Exception e = null);
        void LogFatal(string message, Exception e = null);
    }
}
