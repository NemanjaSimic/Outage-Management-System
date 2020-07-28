using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CE
{
	public static class ReliableDictionaryNames
	{
		//Topology provider
		public static readonly string TopologyCache = "Topology";
		public static readonly string TopologyCacheUI = "TopologyUI";
		public static readonly string TopologyCacheOMS = "TopologyOMS";

		//Model provider
		public static readonly string ElementCache = "ElementCache";
		public static readonly string ElementConnectionCache = "ElementConnectionCache";
		public static readonly string RecloserCache = "RecloserCache";
		public static readonly string EnergySourceCache = "EnergySourceCache";

		//Measurement provider
		public static readonly string AnalogMeasurementsCache = "AnalogMeasurementsCache";
		public static readonly string DiscreteMeasurementsCache = "DiscreteMeasurementsCache";
		public static readonly string ElementsToMeasurementMapCache = "ElementsToMeasurementMapCache";
		public static readonly string MeasurementsToElementMapCache = "MeasurementsToElementMapCache";
	}
}
