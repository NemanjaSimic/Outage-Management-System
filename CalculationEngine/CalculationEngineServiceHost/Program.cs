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
			///testiranje funkcionalnosti metoda
			GDAModelHelper mh = new GDAModelHelper();
			var models = mh.GetAllModelEntities();
			List<TopologyElement> elements = new List<TopologyElement>();
			TopologyElementFactory fact = new TopologyElementFactory();
			foreach (var item in models)
			{
				if (item.Key != Outage.Common.ModelCode.BASEVOLTAGE && item.Key != Outage.Common.ModelCode.DISCRETE && item.Key != Outage.Common.ModelCode.ANALOG)
				{
					foreach (var gid in item.Value)
					{
						elements.Add(TopologyElementFactory.CreateTopologyElement(gid));
					}
				}
			}

			Association association = new Association() { PropertyId = Outage.Common.ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, Type = Outage.Common.ModelCode.TERMINAL };
			NetworkModelGDA gda = new NetworkModelGDA();
			var es = models[Outage.Common.ModelCode.ENERGYSOURCE].FirstOrDefault();
			var terminal = gda.GetRelatedValues(es, new List<Outage.Common.ModelCode>() { Outage.Common.ModelCode.IDOBJ_GID }, association );
			TopologyConnectivity topologyConnectivity = new TopologyConnectivity();
			RegularNode first = topologyConnectivity.Topology(es);
			Console.ReadLine();
		}
	}
}
