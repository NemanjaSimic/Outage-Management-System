using CECommon.Interfaces;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Providers
{
    public class WebTopologyModelProvider : IWebTopologyModelProvider
    {
        private IWebTopologyBuilder webTopologyBuilder;
        private List<UIModel> uiModel;
        private List<UIModel> UIModel 
        {
            get { return uiModel; }
            set
            {
                uiModel = value;
                WebTopologyModelProviderDelegate?.Invoke(UIModel);
            }
        }
        public WebTopologyModelProviderDelegate WebTopologyModelProviderDelegate { get; set; }

        public WebTopologyModelProvider(IWebTopologyBuilder webTopologyBuilder)
        {
            this.webTopologyBuilder = webTopologyBuilder;
            UIModel = ConvertTopologyToWebTopology(Provider.Instance.TopologyProvider.GetTopologies());
            Provider.Instance.WebTopologyModelProvider = this;
            Provider.Instance.TopologyProvider.ProviderTopologyDelegate += ProviderTopologyDelegate;
        }
        public void ProviderTopologyDelegate(List<ITopology> topologies)
        {
            UIModel = ConvertTopologyToWebTopology(topologies);
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

        private List<UIModel> ConvertTopologyToWebTopology(List<ITopology> topologies)
        {
            List<UIModel> newUiModel = new List<UIModel>();
            foreach (var toplogy in topologies)
            {
                newUiModel.Add(webTopologyBuilder.CreateTopologyForWeb(toplogy));
            }
            return newUiModel;
        }
    }
}
