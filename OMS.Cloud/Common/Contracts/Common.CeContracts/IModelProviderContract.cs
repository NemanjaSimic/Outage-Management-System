using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.ModelProvider
{
    [ServiceContract]
    public interface IModelProviderContract : IService
	{
        [OperationContract]
        Task<List<long>> GetEnergySources();
        [OperationContract]
        Task<Dictionary<long, List<long>>> GetConnections();
        [OperationContract]
        Task<Dictionary<long, ITopologyElement>> GetElementModels();
        [OperationContract]
        Task CommitTransaction();
        [OperationContract]
        Task<bool> PrepareForTransaction();
        [OperationContract]
        Task RollbackTransaction();
        [OperationContract]
        Task<HashSet<long>> GetReclosers();
        [OperationContract]
        Task<bool> IsRecloser(long recloserGid);
    }
}
