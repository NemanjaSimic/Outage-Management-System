using CECommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
