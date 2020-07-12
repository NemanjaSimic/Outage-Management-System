using CECommon.Models;
using System.Collections.Generic;

namespace CECommon.Interfaces
{
    public interface IModelProvider
    {
        List<long> GetEnergySources();
        Dictionary<long, List<long>> GetConnections();
        Dictionary<long, ITopologyElement> GetElementModels();
        void CommitTransaction();
        bool PrepareForTransaction();
        void RollbackTransaction();
        HashSet<long> GetReclosers();
        bool IsRecloser(long recloserGid);
    }
}
