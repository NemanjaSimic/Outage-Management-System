namespace Outage.Common.ServiceProxies
{
    using System.ServiceModel;
    
    public interface IProxyFactory
    {
        TProxy CreateProxy<TProxy, TChannel>(string endpointName) where TChannel : class
                                                                  where TProxy : BaseProxy<TChannel>;

        TProxy CreateProxy<TProxy, TChannel>(object callback, string endpointName) where TChannel : class
                                                                                   where TProxy : DuplexClientBase<TChannel>;
    }
}
