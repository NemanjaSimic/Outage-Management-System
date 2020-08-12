using OMS.Common.PubSub;

namespace Common.CeContracts
{
	public interface ITopologyConverter
    {
        IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology);
        IUIModel ConvertTopologyToUIModel(ITopology topology);
    }
}
