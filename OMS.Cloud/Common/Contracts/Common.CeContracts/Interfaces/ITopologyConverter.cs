using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;

namespace Common.CeContracts
{
    public interface ITopologyConverter
    {
        OutageTopologyModel ConvertTopologyToOMSModel(TopologyModel topology);
        UIModel ConvertTopologyToUIModel(TopologyModel topology);
    }
}
