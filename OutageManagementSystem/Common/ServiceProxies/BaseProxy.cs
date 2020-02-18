using System.ServiceModel;

namespace Outage.Common.ServiceProxies
{
    public abstract class BaseProxy<TChannel> : ClientBase<TChannel> where TChannel : class
    {
        public BaseProxy(string endpointName)
            : base(endpointName)
        {
        }
    }

    public abstract class BaseDuplexProxy<TChannel> : DuplexClientBase<TChannel> where TChannel : class
    {
        public BaseDuplexProxy(object callback, string endpointName)
            : base(callback, endpointName)
        {
        }
    }
}
