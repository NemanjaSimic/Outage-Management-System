using Outage.Common.ServiceContracts;
using Outage.Common.UI;

namespace OMS.Web.Adapter.Topology
{
    using OMS.Web.Adapter.Contracts;
    using System;
    using System.ServiceModel;

    [Obsolete("Use Outage.Common.ServiceProxies.CalcualtionEngine.UITopologyServiceProxy instead.")]
    public class TopologyClientProxy : ChannelFactory<ITopologyServiceContract>, ITopologyClient
    {
        private ITopologyServiceContract proxy;

        [Obsolete("Use Outage.Common.ServiceProxies.CalcualtionEngine.UITopologyServiceProxy instead.")]

        public TopologyClientProxy(string address) : base(binding: new NetTcpBinding(SecurityMode.None), remoteAddress: address)
        {
            proxy = this.CreateChannel();
        }

        [Obsolete("Use Outage.Common.ServiceProxies.CalcualtionEngine.UITopologyServiceProxy instead.")]

        public UIModel GetTopology()
        {
            return proxy.GetTopology();
        }
    }
}
