using CECommon;
using NetworkModelServiceFunctions;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
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
			List<long> resourseSources = topologyConnectivity.CreateTopology();
			foreach (var rs in resourseSources)
			{
				topologyConnectivity.PrintTopology(rs);
			}

			Console.ReadLine();
		}
	}
}
