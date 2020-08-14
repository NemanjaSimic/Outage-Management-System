using Common.PubSubContracts.DataContracts.CE.Interfaces;
using OMS.Common.PubSubContracts.Interfaces;
using System.Collections.Generic;

namespace Common.CeContracts
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
