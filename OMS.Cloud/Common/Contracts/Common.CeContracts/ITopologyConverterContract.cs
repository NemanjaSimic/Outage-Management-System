using CECommon;
using CECommon.Interface;
using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	public interface ITopologyConverterContract : IService
	{
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);
		Task<UIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
