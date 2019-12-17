using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PubSubCommon.Proxy
{
    public class PublisherProxy : ClientBase<IPublisher>, IPublisher, IDisposable
    {
        private IPublisher proxy;
        public PublisherProxy(string endPointConfiguration) : base(endPointConfiguration)
        {
            proxy = this.CreateChannel();
        }

        public void Publish(Publication publication)
        {
            try
            {
                proxy.Publish(publication);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            if (proxy != null)
            {
                proxy = null;
            }
            this.Close();
        }
    }
}
