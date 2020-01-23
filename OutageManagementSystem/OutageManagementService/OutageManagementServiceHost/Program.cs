using Outage.Common;
using System;

namespace OutageManagementServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger Logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Outage Management Service...";
                Logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (OutageManagementService.OutageManagementService outageManagementService = new OutageManagementService.OutageManagementService())
                {
                    outageManagementService.Start();

                    message = "Press <Enter> to stop the service";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Outage Management Service failed.");
                Console.WriteLine(ex.StackTrace);
                Logger.LogError($"Outage Management Service failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}
