using Outage.Common;
using NetworkModelServiceFunctions;
using System;
using System.Diagnostics;
using Topology;

namespace CalculationEngineServiceHost
{
    class Program
	{
        static void Main(string[] args)
        {
            ILogger Logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Calculation Engine Service...";
                Logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (CalculationEngineService.CalculationEngineService ces = new CalculationEngineService.CalculationEngineService())
                {
                    ces.Start();

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Calculation Engine Service failed.");
                Console.WriteLine(ex.StackTrace);
                Logger.LogError($"Calculation Engine Service failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }

	}
}
