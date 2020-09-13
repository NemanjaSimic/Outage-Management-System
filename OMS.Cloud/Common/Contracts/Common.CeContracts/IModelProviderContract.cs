using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.ModelProvider
{
	[ServiceContract]
    public interface ICeModelProviderContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<List<long>> GetEnergySources();

        [OperationContract]
        Task<Dictionary<long, List<long>>> GetConnections();

        [OperationContract]
        [ServiceKnownType(typeof(TopologyModel))]
        [ServiceKnownType(typeof(EnergyConsumer))]
        [ServiceKnownType(typeof(Feeder))]
        [ServiceKnownType(typeof(Field))]
        [ServiceKnownType(typeof(Recloser))]
        [ServiceKnownType(typeof(SynchronousMachine))]
        [ServiceKnownType(typeof(TopologyElement))]
        Task<Dictionary<long, TopologyElement>> GetElementModels();

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
