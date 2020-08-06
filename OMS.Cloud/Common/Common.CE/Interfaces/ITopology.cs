using System.Collections.Generic;

namespace Common.CE.Interfaces
{
    public interface ITopology
    {
        long FirstNode { get; set; }
		Dictionary<long, ITopologyElement> TopologyElements { get; set; }
		void AddElement(ITopologyElement newElement); 
        bool GetElementByGid(long gid, out ITopologyElement topologyElement);
    }
}
