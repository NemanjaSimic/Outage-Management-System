using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IVoltageFlow
    {
        ITopology CalulateVoltageFlow(long startingElementGid, ITopology topology);
        void UpdateLoadFlow(List<ITopology> topologies);
        List<ITopology> UpdateVoltageFlow(List<long> startingSignalGid, List<ITopology> topologies);
        List<ITopology> UpdateVoltageFlowFromRecloser(List<long> recloserGids, List<ITopology> topologies);
    }
}
