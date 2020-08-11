using Common.CE.Interfaces;
using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSub;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	[ServiceContract]
	public interface ITopologyConverterContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);
		[OperationContract]
		Task<UIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
