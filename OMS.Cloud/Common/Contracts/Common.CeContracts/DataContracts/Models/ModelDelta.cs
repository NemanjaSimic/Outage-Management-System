using System.Collections.Generic;

namespace Common.CeContracts
{
	public class ModelDelta : IModelDelta
	{
		public Dictionary<long, ITopologyElement> TopologyElements { get; set; }
		public Dictionary<long, List<long>> ElementConnections { get; set; }
		public HashSet<long> Reclosers { get; set; }
		public List<long> EnergySources { get; set; }
	}
}
