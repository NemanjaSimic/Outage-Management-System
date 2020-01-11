using Outage.Common;
using System;

namespace Outage.TransactionManagerServiceHost
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ILogger Logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Transaction Manager Service...";
                Logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (TransactionManagerService.TransactionManagerService tms = new TransactionManagerService.TransactionManagerService())
                {
                    tms.Start();

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Transaction Manager Service failed.");
                Console.WriteLine(ex.StackTrace);
                Logger.LogError($"Transaction Manager Service failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}
