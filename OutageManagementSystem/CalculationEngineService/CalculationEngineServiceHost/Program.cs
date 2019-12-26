using CalculationEngineService;
using CECommon;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TopologyElementsFuntions;

namespace CalculationEngineServiceHost
{
	class Program
	{
        static void Main(string[] args)
        {
            ILogger logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Calculation Engine Service...";
                logger.LogInfo(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                //TopologyConnectivity topologyConnectivity = new TopologyConnectivity();
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                //List<TopologyElement> energySources = topologyConnectivity.MakeAllTopologies();
                //stopwatch.Stop();
                //Console.WriteLine(stopwatch.Elapsed.ToString());
                //foreach (var rs in energySources)
                //{
                //    topologyConnectivity.PrintTopology(rs);
                //}

                using (CalculationEngineService.CalculationEngineService ces = new CalculationEngineService.CalculationEngineService())
                {
                    ces.Start();

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Calculation Engine Service failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Calculation Engine Service failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                logger.LogError($"Calculation Engine Service failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
	}
}
