using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.TopologyProvider
{
    [ServiceContract]
    [ServiceKnownType(typeof(OutageTopologyModel))]
    [ServiceKnownType(typeof(OutageTopologyElement))]
    public interface ITopologyProviderContract : IService, IHealthChecker
    {
        [OperationContract]
        //[ServiceKnownType(typeof(TopologyModel))]
        [ServiceKnownType(typeof(EnergyConsumer))]
        [ServiceKnownType(typeof(Feeder))]
        [ServiceKnownType(typeof(Field))]
        [ServiceKnownType(typeof(Recloser))]
        [ServiceKnownType(typeof(SynchronousMachine))]
        [ServiceKnownType(typeof(TopologyElement))]
        Task<TopologyModel> GetTopology();

        [OperationContract]
        Task CommitTransaction();

        [OperationContract]
        Task<bool> PrepareForTransaction();

        [OperationContract]
        Task RollbackTransaction();

        [OperationContract]
        Task<bool> IsElementRemote(long elementGid);

        [OperationContract]
        Task ResetRecloser(long recloserGid);

        [OperationContract]
        Task RecloserOpened(long recloserGid);

        [OperationContract]
        Task<int> GetRecloserCount(long recloserGid);

        [OperationContract]
        //[ServiceKnownType(typeof(OutageTopologyModel))]
        //[ServiceKnownType(typeof(OutageTopologyElement))]
        Task<OutageTopologyModel> GetOMSModel();

        [OperationContract]
        //[ServiceKnownType(typeof(UIModel))]
        //[ServiceKnownType(typeof(UIMeasurement))]
	    //[ServiceKnownType(typeof(UINode))]
        Task<UIModel> GetUIModel();

        [OperationContract]
        Task DiscreteMeasurementDelegate();
    }
}
