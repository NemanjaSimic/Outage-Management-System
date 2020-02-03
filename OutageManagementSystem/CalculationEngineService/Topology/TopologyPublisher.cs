using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies.PubSub;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topology
{
    public class TopologyPublisher
    {
        private ILogger logger = LoggerWrapper.Instance;
        public TopologyPublisher()
        {
            Provider.Instance.WebTopologyModelProvider.WebTopologyModelProviderDelegate += WebTopologyModelProviderDelegate;
        }
        public void WebTopologyModelProviderDelegate(List<UIModel> uIModels)
        {
            //Dok se ne sredi logika za vise root-ova na WEB-u
            TopologyForUIMessage message = new TopologyForUIMessage(uIModels.First());
            CalculationEnginePublication publication = new CalculationEnginePublication(Topic.TOPOLOGY, message);
            try
            {
                using (var publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint))
                {
                    publisherProxy.Publish(publication);
                    logger.LogDebug("TopologyManager published new topology successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"TopologyManager failed to publish new topology. Exception: {ex.Message}");
            }
        }
    }
}
