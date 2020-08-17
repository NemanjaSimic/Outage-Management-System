using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
    [DataContract]
	[KnownType(typeof(EnergyConsumer))]
	[KnownType(typeof(Feeder))]
	[KnownType(typeof(Field))]
	[KnownType(typeof(Recloser))]
	[KnownType(typeof(SynchronousMachine))]
	[KnownType(typeof(TopologyElement))]
	//TODO: clean up
	public class TopologyModel// : ITopology
    {
		[DataMember]
		public long FirstNode { get; set; }
		[DataMember]
		public Dictionary<long, ITopologyElement> TopologyElements { get; set; }
		
		public TopologyModel()
		{
			TopologyElements = new Dictionary<long, ITopologyElement>();
		}

		public void AddElement(ITopologyElement newElement)
		{
			if (!TopologyElements.ContainsKey(newElement.Id))
			{
				TopologyElements.Add(newElement.Id, newElement);
			}
			else
			{
				//Logger.Instance.LogWarn($"Topology element with GID 0x{newElement.Id.ToString("X16")} is already added.");
			}
		}
		public bool GetElementByGid(long gid, out ITopologyElement topologyElement)
		{
			bool success = false;
			if (TopologyElements.ContainsKey(gid))
			{
				topologyElement = TopologyElements[gid];
				success = true;
			}
			else
			{
				topologyElement = null;
			}
			return success;
		}

	}
}
