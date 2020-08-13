using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.CeContracts
{ 
    public interface ITopology
    {
        long FirstNode { get; set; }
		Dictionary<long, ITopologyElement> TopologyElements { get; set; }
		void AddElement(ITopologyElement newElement); 
        bool GetElementByGid(long gid, out ITopologyElement topologyElement);
    }
}
