
using CECommon.CeContrats;
using CECommon.Interface;

namespace CECommon.Interfaces
{
    public interface ITopologyConverter
    {
        IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology);
        UIModel ConvertTopologyToUIModel(ITopology topology);
    }
}
