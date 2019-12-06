using CECommon;
using NetworkModelServiceFunctions;
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
			ModelHelper mh = new ModelHelper();
			var models = mh.GetAllModelEntities();
			List<TopologyElement> elements = new List<TopologyElement>();
			TopologyElementFactory fact = new TopologyElementFactory();
			foreach (var item in models)
			{
				foreach (var gid in item.Value)
				{
					elements.Add(fact.CreateElement(item.Key, gid));
				}
			}
			Console.ReadLine();
		}
	}
}
