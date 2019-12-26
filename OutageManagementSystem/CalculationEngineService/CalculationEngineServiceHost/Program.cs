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
            ILogger logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Calculation Engine Service...";
                logger.LogInfo(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);
			
				Topology.Topology.Instance.UpdateTopology();

				//PrintTopology(Topology.Topology.Instance.TopologyModel.FirstNode);
				//Console.WriteLine("///////////////////////////////////////////////////////////////////////////////");

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
                Console.WriteLine("CalculationEngineService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "CalculationEngineService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                logger.LogError($"CalculationEngineService failed.{Environment.NewLine}Message: {ex.Message} ", ex);
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
