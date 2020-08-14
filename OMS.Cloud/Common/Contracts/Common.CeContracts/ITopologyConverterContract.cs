using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.Interfaces;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.Interfaces;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
    [ServiceContract]
	[ServiceKnownType(typeof(TopologyModel))]
	[ServiceKnownType(typeof(EnergyConsumer))]
	[ServiceKnownType(typeof(Feeder))]
	[ServiceKnownType(typeof(Field))]
	[ServiceKnownType(typeof(Recloser))]
	[ServiceKnownType(typeof(SynchronousMachine))]
	[ServiceKnownType(typeof(TopologyElement))]
	public interface ITopologyConverterContract : IService, IHealthChecker
	{
		[OperationContract]
		[ServiceKnownType(typeof(OutageTopologyModel))]
		[ServiceKnownType(typeof(OutageTopologyElement))]
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);

		[OperationContract]
		[ServiceKnownType(typeof(UIModel))]
		Task<IUIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
