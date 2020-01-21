using CalculationEngineService;
using Outage.Common;
﻿using CECommon;
using CECommon.Model;
using NetworkModelServiceFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TopologyBuilder;
using TopologyElementsFuntions;
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

                Logger.LogDebug("Initializing NMSManager...");
                NMSManager.Instance.Initialize();
                Logger.LogDebug("NMSManager has been successfully initialized.");

                Logger.LogInfo("Initializing topology...");
                TopologyManager.Instance.InitializeTopology();
                Logger.LogInfo("Topology has been successfully initialized.");

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
