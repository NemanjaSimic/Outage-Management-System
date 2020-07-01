using Serilog;
using Serilog.Events;
using System;
using System.Configuration;
using System.IO;

namespace OMS.Common.Cloud.Logger
{
    internal class CloudLogger : ICloudLogger
    {
        private const LogEventLevel sharedLogLevel = LogEventLevel.Debug;

        private readonly string sourceName;
        private readonly ILogger sharedSerilogLogger;
        private readonly ILogger serviceSerilogLogger;

        private const string logLevelSettingKey = "logLevelSettingKey";
        private const string logFilePathSettingKey = "logFilePathSettingKey";

        internal CloudLogger(string sourceName)
        {
            this.sourceName = sourceName;

            var logOutputTemplate = "{NewLine}{NewLine}{NewLine}{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}]{NewLine}{Message}{NewLine}{Exception}";
            var sharedLogFilePath = GetSharedLogFilePath();
            var serviceLogFilePath = GetServiceLogFilePath(this.sourceName);
            var logLevel = GetLogLevel();

            //TODO istraziti
            //new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();

            this.sharedSerilogLogger = new LoggerConfiguration().WriteTo.RollingFile(pathFormat: sharedLogFilePath,
                                                                                     restrictedToMinimumLevel: sharedLogLevel,
                                                                                     outputTemplate: logOutputTemplate,
                                                                                     retainedFileCountLimit: null,
                                                                                     fileSizeLimitBytes: 52430000, //50 MiB
                                                                                     shared: true).CreateLogger();

            this.serviceSerilogLogger = new LoggerConfiguration().WriteTo.RollingFile(pathFormat: serviceLogFilePath,
                                                                                      restrictedToMinimumLevel: logLevel,
                                                                                      outputTemplate: logOutputTemplate,
                                                                                      retainedFileCountLimit: null,
                                                                                      fileSizeLimitBytes: 52430000, //50 MiB
                                                                                      shared: false).CreateLogger();
        }

        //ako se putanjom cilja fajl unutar C: direktorijuma
        private string GetSharedLogFilePath()
        {
            //ako se sloution nalazi unutar C: direktorijuma nista se nece desiti
            //TOOD: testirati sa solutionom prekopiranim u neki drugi direktorijum
            string logFilePath = $@"..\..\..\..\..\..\LogFiles\SharedLogFile.txt";

            if (ConfigurationManager.AppSettings[logFilePathSettingKey] is string)
            {
                string baseDirectory = ConfigurationManager.AppSettings[logFilePathSettingKey];

                if (Directory.Exists(baseDirectory))
                {
                    return $@"{baseDirectory}\LogFiles\SharedLogFile.txt";
                }
            }

            return logFilePath;
        }

        //ako se putanjom cilja fajl unutar C: direktorijuma
        private string GetServiceLogFilePath(string sourceName)
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
                serviceSerilogLogger.Verbose(MessageFormat(message));
                sharedSerilogLogger.Verbose(MessageFormat(message));
            }
            else
            {
                serviceSerilogLogger. Verbose(e, MessageFormat(message));
                sharedSerilogLogger.Verbose(e, MessageFormat(message));
            }
        }

        public void LogInformation(string message, Exception e = null)
        {
            if (e == null)
            {
                serviceSerilogLogger.Information(MessageFormat(message));
                sharedSerilogLogger.Information(MessageFormat(message));
            }
            else
            {
                serviceSerilogLogger.Information(e, MessageFormat(message));
                sharedSerilogLogger.Information(e, MessageFormat(message));
            }
        }

        public void LogDebug(string message, Exception e = null)
        {
            if (e == null)
            {
                serviceSerilogLogger.Debug(MessageFormat(message));
                sharedSerilogLogger.Debug(MessageFormat(message));
            }
            else
            {
                serviceSerilogLogger.Debug(e, MessageFormat(message));
                sharedSerilogLogger.Debug(e, MessageFormat(message));
            }
        }

        public void LogWarning(string message, Exception e = null)
        {
            if (e == null)
            {
                serviceSerilogLogger.Warning(MessageFormat(message));
                sharedSerilogLogger.Warning(MessageFormat(message));
            }
            else
            {
                serviceSerilogLogger.Warning(e, MessageFormat(message));
                sharedSerilogLogger.Warning(e, MessageFormat(message));
            }
        }

        public void LogError(string message, Exception e = null)
        {
            if (e == null)
            {
                serviceSerilogLogger.Error(MessageFormat(message));
                sharedSerilogLogger.Error(MessageFormat(message));
            }
            else
            {
                serviceSerilogLogger.Error(e, MessageFormat(message));
                sharedSerilogLogger.Error(e, MessageFormat(message));
            }
        }

        public void LogFatal(string message, Exception e = null)
        {
            if (e == null)
            {
                serviceSerilogLogger.Fatal(MessageFormat(message));
                sharedSerilogLogger.Fatal(MessageFormat(message));
            }
            else
            {
                serviceSerilogLogger.Fatal(e, MessageFormat(message));
                sharedSerilogLogger.Fatal(e, MessageFormat(message));
            }
        }
        #endregion ICloudLogger
    }
}
