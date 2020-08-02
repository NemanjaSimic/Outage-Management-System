using OMS.Common.PubSub;

namespace Common.CE.Interfaces
{
	public interface ITopologyConverter
    {
        IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology);
        IUIModel ConvertTopologyToUIModel(ITopology topology);
    }
}
