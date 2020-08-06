using System.Collections.Generic;

namespace Common.CE.Interfaces
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
