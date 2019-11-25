using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.CIMAdapter.Manager
{
    public enum LogLevel
    {
        Fatal,
        Error,
        Warning,
        Info
    }

    public static class LogManager
    {
        /// <summary> Message for user: "Sorry, an unexpected error occurred.\n\n(Advanced: consult log file for more details.)" </summary>
        public const string DefaultErrorMessage = "Sorry, an unexpected error occurred.\n\n(Advanced: consult log file for more details.)";
        public const string GlobalLogFileName = "adapter_log.txt";

        private static string globalLogFullPath;


        static LogManager()
        {
            globalLogFullPath = string.Format(".{0}{1}", Path.DirectorySeparatorChar, GlobalLogFileName);
        }


        public static void Log(string message, LogLevel logLevel = LogLevel.Error)
        {
            try
            {
                if (!File.Exists(globalLogFullPath))
                {
                    using (FileStream fs = File.Create(globalLogFullPath))
                    {
                        fs.Close();
                    }
                }

                using (StreamWriter w = File.AppendText(globalLogFullPath))
                {
                    AppendToLog(logLevel, message, w);
                    //// close the writer and underlying file.
                    w.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }


        #region private block
        private static void AppendToLog(LogLevel logLevel, string logMessage, TextWriter w)
        {
            w.WriteLine("{0} [{1}] : {2}", logLevel, FormatDateTimeAsSortable(DateTime.Now), logMessage);
            //// update the underlying file.
            w.Flush();
        }

        private static string FormatDateTimeAsSortable(DateTime dateTime)
        {
            string formatedDateTime = string.Empty;
            if (dateTime != null)
            {
                formatedDateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dateTime);
            }
            return formatedDateTime;
        }
        #endregion private block
    }
}
