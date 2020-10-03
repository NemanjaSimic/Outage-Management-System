using System.Collections.Generic;

namespace Common.CeContracts
{ 

    public interface IModelProvider
    {
        List<long> GetEnergySources();
        Dictionary<long, List<long>> GetConnections();
        Dictionary<long, TopologyElement> GetElementModels();
        void CommitTransaction();
        bool PrepareForTransaction();
        void RollbackTransaction();
        HashSet<long> GetReclosers();
        bool IsRecloser(long recloserGid);
    }
}
