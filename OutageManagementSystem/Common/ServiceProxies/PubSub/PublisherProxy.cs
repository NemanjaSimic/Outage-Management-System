using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.PubSub
{
    public class PublisherProxy : BaseProxy<IPublisher>, IPublisher
    {
        public PublisherProxy(string endPointConfiguration) 
            : base(endPointConfiguration)
        {
        }

        public void Publish(IPublication publication)
        {
            try
            {
                Channel.Publish(publication);
            }
            catch (Exception e)
            {
                string message = "Exception in Publish() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw;
            }
        }
    }
}
