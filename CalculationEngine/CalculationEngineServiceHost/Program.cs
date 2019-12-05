using NetworkModelServiceFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceHost
{
	class Program
	{
		static void Main(string[] args)
		{
			ModelHelper mh = new ModelHelper();
			var models = mh.GetAllModelEntities();
			Console.ReadLine();
		}
	}
}
