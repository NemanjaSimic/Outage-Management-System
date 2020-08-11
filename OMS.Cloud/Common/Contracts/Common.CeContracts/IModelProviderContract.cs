using Common.CE.Interfaces;
using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.ModelProvider
{
	[ServiceContract]
    public interface IModelProviderContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<List<long>> GetEnergySources();
        [OperationContract]
        Task<Dictionary<long, List<long>>> GetConnections();
        [OperationContract]
        Task<Dictionary<long, ITopologyElement>> GetElementModels();
        [OperationContract]
        Task Commit();
        [OperationContract]
        Task<bool> Prepare();
        [OperationContract]
        Task Rollback();
        [OperationContract]
        Task<HashSet<long>> GetReclosers();
        [OperationContract]
        Task<bool> IsRecloser(long recloserGid);
    }
}
