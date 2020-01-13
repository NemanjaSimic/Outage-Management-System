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

                logger.LogInfo("Initializing topology...");
                Topology.Topology.Instance.InitializeTopology();
                logger.LogInfo("Topology has been successfully initialized.");

                //PrintTopology(Topology.Topology.Instance.TopologyModel.FirstNode);
                //Console.WriteLine("///////////////////////////////////////////////////////////////////////////////");

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


		static void PrintTopology(TopologyElement firstElement)
		{
			foreach (var connectedElement in firstElement.SecondEnd)
			{
				Console.WriteLine($"{TopologyHelper.Instance.GetDMSTypeOfTopologyElement(firstElement.Id)} with gid {firstElement.Id.ToString("X")} connected to {TopologyHelper.Instance.GetDMSTypeOfTopologyElement(connectedElement.Id)} with gid {connectedElement.Id.ToString("X")}");
				PrintTopology(connectedElement);
			}
		}
	}
}
