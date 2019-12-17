using System;

namespace OMS.Web.Common.Interfaces.Logger
{
    public interface ILogger
    {
        void LogDebug(string message);
        void LogError(Exception e, string message = null);
    }
}
