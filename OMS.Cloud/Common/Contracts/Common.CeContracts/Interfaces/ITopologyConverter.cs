using Common.PubSubContracts.DataContracts.CE.Interfaces;
using OMS.Common.PubSubContracts.Interfaces;

namespace Common.CeContracts
{
    public interface ITopologyConverter
    {
        IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology);
        IUIModel ConvertTopologyToUIModel(ITopology topology);
    }
}
