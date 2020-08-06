using OMS.Common.Cloud.Names;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace OMS.Common.Cloud.Logger
{
    public static class CloudLoggerFactory
    {
        private const string loggerSourceNameKey = "loggerSourceNameKey";
        private static readonly object lockSync = new object();

        private static Dictionary<string, CloudLogger> loggers;
        private static Dictionary<string, CloudLogger> Loggers
        {
            get
            {
                return loggers ?? (loggers = new Dictionary<string, CloudLogger>());
            }
        }

        public static ICloudLogger GetLogger(string sourceName = null)
        {
            sourceName = ResolveSourceName(sourceName);

            if (!Loggers.ContainsKey(sourceName))
            {
                lock(lockSync)
                {
                    if (!Loggers.ContainsKey(sourceName))
                    {
                        var logger = new CloudLogger(sourceName);
                        Loggers.Add(sourceName, logger);
                    }
                }
            }

            return Loggers[sourceName];
        }

        private static string ResolveSourceName(string sourceName)
        {
            if (sourceName == null)
            {
                if (ConfigurationManager.AppSettings[loggerSourceNameKey] is string loggerSourceNameValue)
                {
                    sourceName = loggerSourceNameValue;
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{loggerSourceNameKey}' not found in appSettings.");
                }
            }

            //values of public and static fields, from class LoggerSourceNames, of string type, grouped into hashset<string>
            HashSet<string> loggerSourceNames = typeof(LoggerSourceNames).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                                         .Where(f => f.FieldType == typeof(string))
                                                                         .Select(f => (string)f.GetValue(null))
                                                                         .ToHashSet();
            if (!loggerSourceNames.Contains(sourceName))
            {
                sourceName = "Unknown";
            }

            return sourceName;
        }
    }
}
