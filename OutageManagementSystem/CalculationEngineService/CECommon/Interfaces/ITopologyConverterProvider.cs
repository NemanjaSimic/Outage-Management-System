using Outage.Common.OutageService.Interface;
using Outage.Common.UI;
using System.Collections.Generic;

namespace CECommon.Interfaces
{
    public delegate void TopologyConverterToUIModelProviderDelegate(List<UIModel> uiModels);
    public delegate void TopologyConverterToOMSModelProviderDelegate(List<IOutageTopologyModel> omsModels);
    public interface ITopologyConverterProvider
    {
        TopologyConverterToUIModelProviderDelegate TopologyConverterToUIModelProviderDelegate { get; set; }
        TopologyConverterToOMSModelProviderDelegate TopologyConverterToOMSModelProviderDelegate { get; set; }

        List<IOutageTopologyModel> GetOMSModel();
        List<UIModel> GetUIModels();
    }
}
