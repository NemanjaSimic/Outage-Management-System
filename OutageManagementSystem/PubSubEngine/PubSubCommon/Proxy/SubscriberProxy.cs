using System;
using System.ServiceModel;

namespace PubSubCommon.Proxy
{
    public class SubscriberProxy : DuplexClientBase<ISubscriber>, ISubscriber
    {
        private ISubscriber proxy;
        public SubscriberProxy(INotify callBackInstance , string endPointConfiguration) : base(callBackInstance, endPointConfiguration)
        {
            proxy = this.CreateChannel();
        }

        public void Subscribe(Enums.Topic topic)
        {
            try
            {
                proxy.Subscribe(topic);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CloseConnection()
        {
            if (proxy != null)
            {
                proxy = null;
            }
            this.Close();
        }
    }
}
