using CECommon.Providers;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.ServiceProxies.PubSub;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Topology
{
    public class TopologyPublisher
    {
        private ILogger logger = LoggerWrapper.Instance;
        public TopologyPublisher()
        {
            Provider.Instance.TopologyConverterProvider.TopologyConverterToUIModelProviderDelegate += WebTopologyModelProviderDelegate;
            Provider.Instance.TopologyConverterProvider.TopologyConverterToOMSModelProviderDelegate += TopologyToOMSConvertDelegate;
        }
        public void WebTopologyModelProviderDelegate(List<UIModel> uIModels)
        {
            //Dok se ne sredi logika za vise root-ova na WEB-u
            UIModel uIModel;
            if (uIModels.Count == 0)
            {
                uIModel = new UIModel();
            }
            else
            {
                uIModel = uIModels.First();
            }
            TopologyForUIMessage message = new TopologyForUIMessage(uIModel);
            CalculationEnginePublication publication = new CalculationEnginePublication(Topic.TOPOLOGY, message);
            try
            {
                using (var publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint))
                {
                    publisherProxy.Publish(publication);
                    logger.LogDebug("Topology publisher published new ui model successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Topology publisher failed to publish new ui model. Exception: {ex.Message}");
            }
        }

        public void TopologyToOMSConvertDelegate(List<IOutageTopologyModel> outageTopologyModels)
        {
            IOutageTopologyModel outageTopologyModel;
            if (outageTopologyModels.Count == 0)
            {
                outageTopologyModel = new OutageTopologyModel();
            }
            else
            {
                outageTopologyModel = outageTopologyModels.First();
            }

            OMSModelMessage message = new OMSModelMessage(outageTopologyModel);
            CalculationEnginePublication publication = new CalculationEnginePublication(Topic.OMS_MODEL, message);
            try
            {
                using (var publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint))
                {
                    publisherProxy.Publish(publication);
                    logger.LogDebug("Topology publisher published new oms model successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Topology publisher failed to publish new oms model. Exception: {ex.Message}");
            }

        }

    }
}
