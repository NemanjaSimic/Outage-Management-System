using OMS.Common.PubSub;
using System.Collections.Generic;

namespace Common.CE.Interfaces
{
	public delegate void TopologyConverterToUIModelProviderDelegate(List<IUIModel> uiModels);
    public delegate void TopologyConverterToOMSModelProviderDelegate(List<IOutageTopologyModel> omsModels);
    public interface ITopologyConverterProvider
    {
        TopologyConverterToUIModelProviderDelegate TopologyConverterToUIModelProviderDelegate { get; set; }
        TopologyConverterToOMSModelProviderDelegate TopologyConverterToOMSModelProviderDelegate { get; set; }

        List<IOutageTopologyModel> GetOMSModel();
        List<IUIModel> GetUIModels();
    }
}
