using System.Collections.Generic;

namespace CECommon.Interfaces
{
    public interface ILoadFlow
    {
        void UpdateLoadFlow(List<ITopology> topologies);
    }
}
