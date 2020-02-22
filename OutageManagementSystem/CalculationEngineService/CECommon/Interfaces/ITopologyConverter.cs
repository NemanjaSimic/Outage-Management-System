using Outage.Common.OutageService.Interface;
using Outage.Common.UI;

namespace CECommon.Interfaces
{
    public interface ITopologyConverter
    {
        IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology);
        UIModel ConvertTopologyToUIModel(ITopology topology);
    }
}
