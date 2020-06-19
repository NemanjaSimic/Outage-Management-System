using Serilog;
using Serilog.Events;
using System;
using System.Configuration;
using System.IO;

namespace OMS.Common.Cloud.Logger
{
    internal class CloudLogger : ICloudLogger
    {
        private readonly string sourceName;
        private readonly ILogger serilogLogger;

        private const string logLevelSettingKey = "logLevelSettingKey";
        private const string logFilePathSettingKey = "logFilePathSettingKey";

        internal CloudLogger(string sourceName)
        {
            this.sourceName = sourceName;

            var logOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";
            var logFilePath = GetLogFilePath(this.sourceName);
            var logLevel = GetLogLevel();

            this.serilogLogger = new LoggerConfiguration().WriteTo.RollingFile(pathFormat: logFilePath,
                                                                               restrictedToMinimumLevel: logLevel,
                                                                               outputTemplate: logOutputTemplate,
                                                                               retainedFileCountLimit: null,
                                                                               fileSizeLimitBytes: 52430000, //50 MiB
                                                                               shared: true).CreateLogger();
        }

        //ako se putanjom cilja fajl unutar C: direktorijuma
        private string GetLogFilePath(string sourceName)
        {
            //ako se sloution nalazi unutar C: direktorijuma nista se nece desiti
            //TOOD: testirati sa solutionom prekopiranim u neki drugi direktorijum
            string logFilePath = $@"..\..\..\..\..\..\LogFiles\{sourceName}LogFile.txt";

            if (ConfigurationManager.AppSettings[logFilePathSettingKey] is string)
            {
                string baseDirectory = ConfigurationManager.AppSettings[logFilePathSettingKey];
                
                if(Directory.Exists(baseDirectory))
                {
                    return $@"{baseDirectory}\LogFiles\{sourceName}LogFile.txt";
                }       
            }

            return logFilePath;
        }

        private LogEventLevel GetLogLevel()
        {
            string logLevelSetting = "";
            
            if (ConfigurationManager.AppSettings[logLevelSettingKey] is string)
            {
                logLevelSetting = ConfigurationManager.AppSettings[logLevelSettingKey];
            }

            switch (logLevelSetting)
            {
                case "Fatal":
                    return LogEventLevel.Fatal;
                case "Error":
                    return LogEventLevel.Error;
                case "Warning":
                    return LogEventLevel.Warning;
                case "Information":
                    return LogEventLevel.Information;
                case "Debug":
                    return LogEventLevel.Debug;
                case "Verbose":
                    return LogEventLevel.Verbose;
                default:
                    return LogEventLevel.Information;
            }
        }

        private string MessageFormat(string message)
        {
            return $"[{sourceName}] => {message}";
        }

        #region ICloudLogger
        public void LogVerbose(string message, Exception e = null)
        {
            if (e == null)
            {
                serilogLogger.Verbose(MessageFormat(message));
            }
            else
            {
                serilogLogger.Verbose(e, MessageFormat(message));
            }
        }

        public void LogInformation(string message, Exception e = null)
        {
            if (e == null)
            {
                serilogLogger.Information(MessageFormat(message));
            }
            else
            {
                serilogLogger.Information(e, MessageFormat(message));
            }
        }

        public void LogDebug(string message, Exception e = null)
        {
            if (e == null)
            {
                serilogLogger.Debug(MessageFormat(message));
            }
            else
            {
                serilogLogger.Debug(e, MessageFormat(message));
            }
        }

        public void LogWarning(string message, Exception e = null)
        {
            if (e == null)
            {
                serilogLogger.Warning(MessageFormat(message));
            }
            else
            {
                serilogLogger.Warning(e, MessageFormat(message));
            }
        }

        public void LogError(string message, Exception e = null)
        {
            if (e == null)
            {
                serilogLogger.Error(MessageFormat(message));
            }
            else
            {
                serilogLogger.Error(e, MessageFormat(message));
            }
        }

        public void LogFatal(string message, Exception e = null)
        {
            if (e == null)
            {
                serilogLogger.Fatal(MessageFormat(message));
            }
            else
            {
                serilogLogger.Fatal(e, MessageFormat(message));
            }
        }
        #endregion ICloudLogger
    }
}
