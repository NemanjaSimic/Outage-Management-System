using Common.CE.Interfaces;
using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSub;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.TopologyProvider
{
	[ServiceContract]
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
