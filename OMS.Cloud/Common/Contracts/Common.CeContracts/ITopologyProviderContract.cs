using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.Interfaces;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.Interfaces;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.TopologyProvider
{
    [ServiceContract]
    public interface ITopologyProviderContract : IService, IHealthChecker
    {
        [OperationContract]
        [ServiceKnownType(typeof(TopologyModel))]
        [ServiceKnownType(typeof(EnergyConsumer))]
        [ServiceKnownType(typeof(Feeder))]
        [ServiceKnownType(typeof(Field))]
        [ServiceKnownType(typeof(Recloser))]
        [ServiceKnownType(typeof(SynchronousMachine))]
        [ServiceKnownType(typeof(TopologyElement))]
        Task<ITopology> GetTopology();

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
        [ServiceKnownType(typeof(OutageTopologyModel))]
        [ServiceKnownType(typeof(OutageTopologyElement))]
        Task<IOutageTopologyModel> GetOMSModel();

        [OperationContract]
        [ServiceKnownType(typeof(UIModel))]
        Task<IUIModel> GetUIModel();

        [OperationContract]
        Task DiscreteMeasurementDelegate();
    }
}
