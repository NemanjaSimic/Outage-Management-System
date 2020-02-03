using CECommon.Providers;
using Outage.Common;
using Outage.Common.OutageService.Interface;
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
            TopologyForUIMessage message = new TopologyForUIMessage(uIModels.First()); 
            CalcualtionEnginePublication publication = new CalcualtionEnginePublication(Topic.TOPOLOGY, message);
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
            OMSModelMessage message = new OMSModelMessage(outageTopologyModels.First());
            CalcualtionEnginePublication publication = new CalcualtionEnginePublication(Topic.OMS_MODEL, message);
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
