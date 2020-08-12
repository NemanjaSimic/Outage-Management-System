using Common.CloudContracts;
using Common.OMS.OutageModel;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSub;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	[ServiceContract]
	[ServiceKnownType(typeof(OutageTopologyModel))]
	[ServiceKnownType(typeof(TopologyModel))]
	public interface ITopologyConverterContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);
		[OperationContract]
		Task<UIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
