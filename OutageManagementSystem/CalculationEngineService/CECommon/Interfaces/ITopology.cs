using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface ITopology
    {
        long FirstNode { get; set; }
		Dictionary<long, ITopologyElement> TopologyElements { get; set; }

		void AddElement(ITopologyElement newElement); 
        bool GetElementByGid(long gid, out ITopologyElement topologyElement);
    }
}
