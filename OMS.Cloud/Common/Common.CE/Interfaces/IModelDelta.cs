using Common.CE.Interfaces;
using System.Collections.Generic;

namespace Common.CE.Interfaces
{
	public interface IModelDelta
	{
		Dictionary<long, List<long>> ElementConnections { get; set; }
		List<long> EnergySources { get; set; }
		HashSet<long> Reclosers { get; set; }
		Dictionary<long, ITopologyElement> TopologyElements { get; set; }
	}
}