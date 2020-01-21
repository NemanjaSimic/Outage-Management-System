using Outage.Common;
using System;

namespace Outage.SCADA.SCADAServiceHost
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ILogger Logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting SCADA Service...";
                Logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (SCADAService.SCADAService scadaService = new SCADAService.SCADAService())
                {
                    scadaService.Start();

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("SCADA Service failed.");
                Console.WriteLine(ex.StackTrace);
                Logger.LogError($"SCADA Service failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}