using System.Collections.Generic;

namespace Common.CeContracts
{
    public delegate void ProviderTopologyDelegate(List<TopologyModel> topology);
    public delegate void ProviderTopologyConnectionDelegate(List<TopologyModel> topology);

    public interface ITopologyProvider
    {
        List<TopologyModel> GetTopologies();
        void CommitTransaction();
        bool PrepareForTransaction();
        void RollbackTransaction();
        bool IsElementRemote(long elementGid);
        void ResetRecloser(long recloserGid);
    }
}
