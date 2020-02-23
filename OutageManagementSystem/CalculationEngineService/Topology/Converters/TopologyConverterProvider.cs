using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common.OutageService.Interface;
using Outage.Common.UI;
using System.Collections.Generic;

namespace Topology
{
    public class TopologyConverterProvider : ITopologyConverterProvider
    {
        private ITopologyConverter topologyConverter;
        private List<UIModel> uiModel;
        private List<IOutageTopologyModel> omsModel;
        private List<UIModel> UIModel 
        {
            get { return uiModel; }
            set
            {
                uiModel = value;
                TopologyConverterToUIModelProviderDelegate?.Invoke(UIModel);
            }
        }
        private List<IOutageTopologyModel> OMSModel
        { 
            get => omsModel;
            set
            {
                omsModel = value;
                TopologyConverterToOMSModelProviderDelegate?.Invoke(OMSModel);
            }
        }
        public TopologyConverterToUIModelProviderDelegate TopologyConverterToUIModelProviderDelegate { get; set; }
        public TopologyConverterToOMSModelProviderDelegate TopologyConverterToOMSModelProviderDelegate { get; set; }

        public TopologyConverterProvider(ITopologyConverter topologyConverter)
        {
            this.topologyConverter = topologyConverter;
            UIModel = ConvertTopologyToWebTopology(Provider.Instance.TopologyProvider.GetTopologies());
            OMSModel = ConvertTopologyToOMSModel(Provider.Instance.TopologyProvider.GetTopologies());
            Provider.Instance.TopologyConverterProvider = this;
            Provider.Instance.TopologyProvider.ProviderTopologyDelegate += ProviderTopologyDelegate;
            Provider.Instance.TopologyProvider.ProviderTopologyConnectionDelegate += ProviderTopologyConnectionDelegate;
        }
        public void ProviderTopologyDelegate(List<ITopology> topologies)
        {
            UIModel = ConvertTopologyToWebTopology(topologies);
        }
        public void ProviderTopologyConnectionDelegate(List<ITopology> topologies)
        {
            OMSModel = ConvertTopologyToOMSModel(topologies);
        }

        public List<UIModel> GetUIModels()
        {
            if (UIModel != null)
            {
                return UIModel;
            }
            else
            {
                return new List<UIModel>();
            }
        }
        public List<IOutageTopologyModel> GetOMSModel()
        {
            if (OMSModel != null)
            {
                return OMSModel;
            }
            else
            {
                return new List<IOutageTopologyModel>();
            }
        }
        private List<UIModel> ConvertTopologyToWebTopology(List<ITopology> topologies)
        {
            List<UIModel> newUiModel = new List<UIModel>();
            foreach (var toplogy in topologies)
            {
                newUiModel.Add(topologyConverter.ConvertTopologyToUIModel(toplogy));
            }
            return newUiModel;
        }
        private List<IOutageTopologyModel> ConvertTopologyToOMSModel(List<ITopology> topologies)
        {
            List<IOutageTopologyModel> newUiModel = new List<IOutageTopologyModel>();
            foreach (var toplogy in topologies)
            {
                newUiModel.Add(topologyConverter.ConvertTopologyToOMSModel(toplogy));
            }
            return newUiModel;
        }
    }
}
