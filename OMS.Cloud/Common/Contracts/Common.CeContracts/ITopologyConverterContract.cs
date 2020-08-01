using CECommon;
using CECommon.Interface;
using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	[ServiceContract]
	public interface ITopologyConverterContract : IService
	{
		[OperationContract]
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);
		[OperationContract]
		Task<UIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
