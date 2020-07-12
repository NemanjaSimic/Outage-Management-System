using System.Collections.Generic;

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
