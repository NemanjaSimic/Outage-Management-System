using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
    //todo: clean up
    [ServiceContract]
	//[ServiceKnownType(typeof(TopologyModel))]
	[ServiceKnownType(typeof(TopologyElement))]
	[ServiceKnownType(typeof(EnergyConsumer))]
	[ServiceKnownType(typeof(Feeder))]
	[ServiceKnownType(typeof(Field))]
	[ServiceKnownType(typeof(Recloser))]
	[ServiceKnownType(typeof(SynchronousMachine))]
	//[ServiceKnownType(typeof(OutageTopologyModel))]
	//[ServiceKnownType(typeof(OutageTopologyElement))]
	//[ServiceKnownType(typeof(UIModel))]
	//[ServiceKnownType(typeof(UIMeasurement))]
	//[ServiceKnownType(typeof(UINode))]
	public interface ITopologyConverterContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<OutageTopologyModel> ConvertTopologyToOMSModel(TopologyModel topology);

		[OperationContract]
		Task<UIModel> ConvertTopologyToUIModel(TopologyModel topology);
	}
}
