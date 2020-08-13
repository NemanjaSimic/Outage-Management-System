using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.Interfaces;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.TopologyProvider
{
    [ServiceContract]
    [ServiceKnownType(typeof(TopologyModel))]
    [ServiceKnownType(typeof(OutageTopologyModel))] //REORGANIZOVATI! OutageTopologyModel je iz Common.OMS.dll
    public interface ITopologyProviderContract : IService, IHealthChecker
    {
        [OperationContract]
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
        Task<IOutageTopologyModel> GetOMSModel();
        [OperationContract]
        Task<UIModel> GetUIModel();
        [OperationContract]
        Task DiscreteMeasurementDelegate();
    }
}
