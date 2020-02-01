using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IModelTopologyServis
    {
        List<ITopology> CreateTopology();
        ITopology CalulateLoadFlow(long startingElementGid, ITopology topology);
        List<ITopology> UpdateLoadFlow(long startingSignalGid, List<ITopology> topologies);
    }
}
