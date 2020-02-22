using CECommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IModelManager
    {
        bool TryGetAllModelEntities(
            out Dictionary<long, ITopologyElement> topologyElements, 
            out Dictionary<long, List<long>> elementConnections, 
            out HashSet<long> reclosers, 
            out List<long> energySources);
    }
}
