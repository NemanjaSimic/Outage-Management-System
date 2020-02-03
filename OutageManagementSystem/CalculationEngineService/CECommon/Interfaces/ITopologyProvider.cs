using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public delegate void ProviderTopologyDelegate(List<ITopology> topology);
    public delegate void ProviderTopologyConnectionDelegate(List<ITopology> topology);
    public interface ITopologyProvider
    {
        ProviderTopologyDelegate ProviderTopologyDelegate { get; set; }
        ProviderTopologyConnectionDelegate ProviderTopologyConnectionDelegate { get; set; }
        List<ITopology> GetTopologies();
        void CommitTransaction();
        bool PrepareForTransaction();
        void RollbackTransaction();
        bool IsElementRemote(long elementGid);
    }
}
