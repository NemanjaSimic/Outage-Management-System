using System.Collections.Generic;

namespace Common.CeContracts
{
    public delegate void ProviderTopologyDelegate(List<ITopology> topology);
    public delegate void ProviderTopologyConnectionDelegate(List<ITopology> topology);
    public interface ITopologyProvider
    {
        List<ITopology> GetTopologies();
        void CommitTransaction();
        bool PrepareForTransaction();
        void RollbackTransaction();
        bool IsElementRemote(long elementGid);
        void ResetRecloser(long recloserGid);
    }
}
