using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using System.Collections.Generic;

namespace Common.CeContracts
{
    public delegate void TopologyConverterToUIModelProviderDelegate(List<UIModel> uiModels);
    public delegate void TopologyConverterToOMSModelProviderDelegate(List<OutageTopologyModel> omsModels);
    
    public interface ITopologyConverterProvider
    {
        TopologyConverterToUIModelProviderDelegate TopologyConverterToUIModelProviderDelegate { get; set; }
        TopologyConverterToOMSModelProviderDelegate TopologyConverterToOMSModelProviderDelegate { get; set; }

        List<OutageTopologyModel> GetOMSModel();
        List<UIModel> GetUIModels();
    }
}
