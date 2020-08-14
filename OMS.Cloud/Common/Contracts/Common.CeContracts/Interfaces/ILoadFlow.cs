using System.Collections.Generic;

namespace Common.CeContracts
{
    public interface ILoadFlow
    {
        void UpdateLoadFlow(List<ITopology> topologies);
    }
}
