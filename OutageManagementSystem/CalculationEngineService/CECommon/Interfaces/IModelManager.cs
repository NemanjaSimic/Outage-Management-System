using CECommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IModelManager
    {
        List<long> GetAllEnergySources();
        void GetAllModels(out Dictionary<long, ITopologyElement> elementi, out Dictionary<long, IMeasurement> merenja, out Dictionary<long, List<long>> connections);
        void PrepareTransaction();
    }
}
