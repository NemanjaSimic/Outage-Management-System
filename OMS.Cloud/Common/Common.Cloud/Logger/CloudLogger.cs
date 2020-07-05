using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;

namespace OMS.Common.Cloud.Logger
{
    internal class CloudLogger : ICloudLogger
    {
        private const LogEventLevel sharedLogLevel = LogEventLevel.Debug;

        private readonly string sourceName;
        private readonly IEnumerable<ILogger> serilogLoggers;
        //private readonly ILogger sharedSerilogLogger;
        //private readonly ILogger serviceSerilogLogger;

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
            //var logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();

            var sharedSerilogLogger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                               .WriteTo.RollingFile(pathFormat: sharedLogFilePath,
                                                                                                restrictedToMinimumLevel: sharedLogLevel,
                                                                                                outputTemplate: logOutputTemplate,
                                                                                                retainedFileCountLimit: null,
                                                                                                fileSizeLimitBytes: 52430000, //50 MiB
                                                                                                shared: true).CreateLogger();

            var serviceSerilogLogger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                                .WriteTo.RollingFile(pathFormat: serviceLogFilePath,
                                                                                     restrictedToMinimumLevel: logLevel,
                                                                                     outputTemplate: logOutputTemplate,
                                                                                     retainedFileCountLimit: null,
                                                                                     fileSizeLimitBytes: 52430000, //50 MiB
                                                                                     shared: false).CreateLogger();

            this.serilogLoggers = new List<ILogger>()
            {
                sharedSerilogLogger,
                serviceSerilogLogger,
            };
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
                foreach(var logger in serilogLoggers)
                {
                    logger.Verbose(MessageFormat(message));
                }
            }
            else
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Verbose(e, MessageFormat(message));
                }
            }
        }

        public void LogDebug(string message, Exception e = null)
        {
            if (e == null)
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Debug(MessageFormat(message));
                }
            }
            else
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Debug(e, MessageFormat(message));
                }
            }
        }

        public void LogInformation(string message, Exception e = null)
        {
            if (e == null)
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Information(MessageFormat(message));
                }
            }
            else
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Information(e, MessageFormat(message));
                }
            }
        }

        public void LogWarning(string message, Exception e = null)
        {
            if (e == null)
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Warning(MessageFormat(message));
                }
            }
            else
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Warning(e, MessageFormat(message));
                }
            }
        }

        public void LogError(string message, Exception e = null)
        {
            if (e == null)
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Error(MessageFormat(message));
                }
            }
            else
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Error(e, MessageFormat(message));
                }
            }
        }

        public void LogFatal(string message, Exception e = null)
        {
            if (e == null)
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Fatal(MessageFormat(message));
                }
            }
            else
            {
                foreach (var logger in serilogLoggers)
                {
                    logger.Fatal(e, MessageFormat(message));
                }
            }
        }
        #endregion ICloudLogger
    }
}
