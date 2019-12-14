﻿using CECommon;
using NetworkModelServiceFunctions;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyElementsFuntions;

namespace CalculationEngineServiceHost
{
	class Program
	{
		static void Main(string[] args)
		{
			TopologyConnectivity topologyConnectivity = new TopologyConnectivity();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			List<TopologyElement> energySources = topologyConnectivity.MakeAllTopologies();
			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed.ToString());
			foreach (var rs in energySources)
			{
				topologyConnectivity.PrintTopology(rs);
			}
			Console.ReadLine();
		}

	}
}
