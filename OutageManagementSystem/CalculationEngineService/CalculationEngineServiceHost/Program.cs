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

namespace CalculationEngineServiceHost
{
	class Program
	{
        static void Main(string[] args)
        {
            ILogger logger = LoggerWrapper.Instance;

			//Console.WriteLine("CE started...");
			//Stopwatch stopwatch = new Stopwatch();
			//stopwatch.Start();
			//GDAModelHelper.Instance.RetrieveAllElements();
			//stopwatch.Stop();
			//Console.WriteLine("Elements retrieved for " + stopwatch.Elapsed.ToString());
			//GraphBuilder graphBuilder = new GraphBuilder();
			//List<long> es = GDAModelHelper.Instance.GetAllEnergySousces();
			//stopwatch.Restart();
			//var topology = graphBuilder.CreateGraphTopology(es.First());
			//stopwatch.Stop();
			//Console.WriteLine("Topology created for " + stopwatch.Elapsed.ToString());
			//PrintTopology(topology.FirstNode);
			//Console.WriteLine("///////////////////////////////////////////////////////////////////////////////");
			//PrintUI(topology);
			//Console.ReadLine();
		


		//static void PrintTopology(TopologyElement firstElement)
		//{
		//	foreach (var connectedElement in firstElement.SecondEnd)
		//	{
		//		Console.WriteLine($"{TopologyHelper.Instance.GetDMSTypeOfTopologyElement(firstElement.Id)} with gid {firstElement.Id.ToString("X")} connected to {TopologyHelper.Instance.GetDMSTypeOfTopologyElement(connectedElement.Id)} with gid {connectedElement.Id.ToString("X")}");
		//		PrintTopology(connectedElement);
		//	}
		//}
		//static void PrintUI(Topology topology)
		//{
		//	Print(topology.FirstNode.Id, topology);
		//}

		//static void Print(long parent, Topology topology)
		//{
		//	var connectedElements = topology.GetRelatedElements(parent);
		//	if (connectedElements != null)
		//	{
		//		foreach (var connectedElement in connectedElements)
		//		{
		//			Console.WriteLine($"{TopologyHelper.Instance.GetDMSTypeOfTopologyElement(parent)} with gid {parent.ToString("X")} connected to {TopologyHelper.Instance.GetDMSTypeOfTopologyElement(connectedElement)} with gid {connectedElement.ToString("X")}");
		//			Print(connectedElement, topology);
		//		}
		//	}
		//}

            try
            {
                string message = "Starting Calculation Engine Service...";
                logger.LogInfo(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);          

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
	}
}
