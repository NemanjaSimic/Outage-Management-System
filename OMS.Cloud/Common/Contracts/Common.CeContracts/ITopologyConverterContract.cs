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
	[ServiceKnownType(typeof(OutageTopologyModel))]
	[ServiceKnownType(typeof(OutageTopologyElement))]
	[ServiceKnownType(typeof(UIModel))]
	[ServiceKnownType(typeof(UIMeasurement))]
	[ServiceKnownType(typeof(UINode))]
	public interface ITopologyConverterContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology);

		[OperationContract]
		Task<IUIModel> ConvertTopologyToUIModel(ITopology topology);
	}
}
