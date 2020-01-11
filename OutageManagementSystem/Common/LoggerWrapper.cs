using log4net;
using System;
using System.Configuration;

namespace Outage.Common
{
    public class LoggerWrapper : ILogger
    {
        private static LoggerWrapper instance;
        private static object lockObj = new object();

        private ILog Logger;

        private LoggerWrapper(string callerName)
        {
            Logger = LogManager.GetLogger(callerName);
        }

        public static LoggerWrapper Instance
        {
            get
            {

                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            string callerName = "Unknown";
                            if (ConfigurationManager.ConnectionStrings["LoggerName"] != null)
                            {
                                callerName = ConfigurationManager.ConnectionStrings["LoggerName"].ConnectionString;
                            }

                            instance = new LoggerWrapper(callerName);
                        }
                    }
                }

                return instance;
            }


        }




        public void LogInfo(string message, Exception e = null)
        {
            Logger.Info(message, e);
        }

        public void LogDebug(string message, Exception e = null)
        {
            Logger.Debug(message, e);
        }

        public void LogWarn(string message, Exception e = null)
        {
            Logger.Warn(message, e);
        }

        public void LogError(string message, Exception e = null)
        {
            Logger.Error(message, e);
        }

        public void LogFatal(string message, Exception e = null)
        {
            Logger.Fatal(message, e);
        }
    }
}
