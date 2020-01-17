using OMS.Web.Adapter.Contracts;
using Outage.Common.ServiceContracts;
using Outage.Common.UI;
using System.ServiceModel;

namespace OMS.Web.Adapter.Topology
{
    public class TopologyClientProxy : ChannelFactory<ITopologyServiceContract>, ITopologyClient
    {
        private ITopologyServiceContract proxy;

        public TopologyClientProxy(string address) : base(binding: new NetTcpBinding(SecurityMode.None), remoteAddress: address)
        {
            proxy = this.CreateChannel();
        }

        public UIModel GetTopology()
        {
            return proxy.GetTopology();
        }
    }
}
