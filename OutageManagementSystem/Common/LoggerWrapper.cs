using log4net;
using System;
using System.Configuration;

namespace Outage.Common
{
    public class LoggerWrapper : ILogger
    {
        private static LoggerWrapper instance;
        private static object lockObj = new object();

        private ILog logger;

        private LoggerWrapper(string callerName)
        {
            logger = LogManager.GetLogger(callerName);
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
                            if (ConfigurationManager.ConnectionStrings["loggerName"] != null)
                            {
                                callerName = ConfigurationManager.ConnectionStrings["loggerName"].ConnectionString;
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
            logger.Info(message, e);
        }

        public void LogDebug(string message, Exception e = null)
        {
            logger.Debug(message, e);
        }

        public void LogWarn(string message, Exception e = null)
        {
            logger.Warn(message, e);
        }

        public void LogError(string message, Exception e = null)
        {
            logger.Error(message, e);
        }

        public void LogFatal(string message, Exception e = null)
        {
            logger.Fatal(message, e);
        }
    }
}
