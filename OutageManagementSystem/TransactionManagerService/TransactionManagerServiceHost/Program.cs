using Outage.Common;
using System;

namespace Outage.TransactionManagerServiceHost
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ILogger logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Transaction Manager Service...";
                logger.LogInfo(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (TransactionManagerService.TransactionManagerService tms = new TransactionManagerService.TransactionManagerService())
                {
                    tms.Start();

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("TransactionManagerService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "TransactionManagerService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                logger.LogError($"TransactionManagerService failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}
