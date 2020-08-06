using System.Collections.Generic;

namespace Common.CE.Interfaces
{
    public interface ILoadFlow
    {
        void UpdateLoadFlow(List<ITopology> topologies);
    }
}
