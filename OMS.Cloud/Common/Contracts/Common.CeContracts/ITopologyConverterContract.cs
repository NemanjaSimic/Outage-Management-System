using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.Interfaces;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
    [ServiceContract]
	[ServiceKnownType(typeof(OutageTopologyModel))] //REORGANIZOVATI! OutageTopologyModel je iz Common.OMS.dll
	[ServiceKnownType(typeof(TopologyModel))]
	public interface ITopologyConverterContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);
		[OperationContract]
		Task<UIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
