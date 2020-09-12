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
		public static readonly string ModelChanges = "ModelChanges";

		//Model provider - ModelManager
		public static readonly string EnergySources= "EnergySources";
		public static readonly string Reclosers = "Reclosers";
		public static readonly string Measurements = "Measurements";
		public static readonly string TopologyElements = "TopologyElements";
		public static readonly string BaseVoltages = "BaseVoltages";
		public static readonly string ElementConnections = "ElementConnections";
		public static readonly string MeasurementToConnectedTerminalMap = "MeasurementToConnectedTerminalMap";
		public static readonly string TerminalToConnectedElementsMap = "TerminalToConnectedElementsMap";

		//Measurement provider
		public static readonly string AnalogMeasurementsCache = "AnalogMeasurementsCache";
		public static readonly string DiscreteMeasurementsCache = "DiscreteMeasurementsCache";
		public static readonly string ElementsToMeasurementMapCache = "ElementsToMeasurementMapCache";
		public static readonly string MeasurementsToElementMapCache = "MeasurementsToElementMapCache";
	}
}
